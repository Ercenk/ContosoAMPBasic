# ContosoAMPBasic sample for Azure Marketplace SaaS integration

This sample demonstrates the basic interaction of a SaaS solution with Azure Marketplace. It does not have any SaaS functionality, however it is a bare bones approach focusing on the marketplace integration.

Please see the related [section](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient#integrating-a-software-as-a-solution-with-azure-marketplace) on my marketplace REST API client implementation for an overview of the integration concepts.  

This sample can be a good starting point if the solution does not have requirements for providing native experience for cancelling and updating a subscription.

It exposes a landing page that can be customized for branding. It provides a webhook endpoint for processing the incoming notifications from the Azure Marketplace. The rest of the integration is done via emails.

The landing page also includes questions to be answered, for example what is the favored region. When a subscriber provides the details on the landing page, the solution generates an email to the configured operations contact. The operations team then provisions the required resources and onboards the customer using their internal processes then comes back to the generated email and clicks on the link in the email to activate the subscription.

The solution does not implement native "cancel" or "update" subscription options. The subscriber uses the Azure Marketplace to cancel or update the subscription. In the case of a change on the subscription (cancel, update, suspend), the marketplace posts a notification to the webhook, and the operations contact receives an email. Just like the onboarding operation, the operations contact performs the required operation manually, and then returns to the email, and clicks on the included link to indicate completion of the operation.

I give an overview of integrating a SaaS application with Azure Marketplace in my sample client library [sample](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient). I point out three areas for integration.

- [Landing page](https://github.com/Ercenk/ContosoAMPBasic/blob/master/src/Dashboard/Controllers/LandingPageController.cs#L27)
- [Webhook endpoint](https://github.com/Ercenk/ContosoAMPBasic/blob/master/src/Dashboard/Controllers/WebHookController.cs)
- [Calling the API](https://github.com/Ercenk/ContosoAMPBasic/blob/master/src/Dashboard/Controllers/LandingPageController.cs#L19)
 
![overview](./Docs/Overview.png)

## Notes

## Secrets

Secrets such as API keys are managed through "dotnet user-secrets" command. For example, to set the value for "FulfillmentClient:AzureActiveDirectory:AppKey" use the following command:

``` sh
dotnet user-secrets set "FulfillmentClient:AzureActiveDirectory:AppKey" "secret here"
```

Please see the user secrets [documentation](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows) for more details.

# How to deploy and run?
How do I run the sample?

The top level actions are:
1. Create and configure Azure Active Directory applications.
1. Create and configure a SendGrid account.
1. Update user secrets.

## Registering Azure Active Directory applications

It's a best practice to  maintain a separate Azure Active Directory tenant (directory) for my application registrations. To create one, 

1. Login to Azure [portal](https://portal.azure.com)
1. Click "Create a resource", and type in "azure active directory" in the search box, and select

    ![createdirectory](./Docs/createdirectory.png)

    Then fill in the details as you see fit after clicking the "create" button. The result is a new directory.
1. Switch to the new directory.

    ![switchdirectory](./Docs/switchdirectory.png)
1. Select the new directory, if it does not show under "Favorites" check "All directories" 

    ![gotodirectory](./Docs/gotodirectory.png)

Once you switch to the new directory (or if you have not created a new one, and decided to use the existing one instead), select the Active Directory service (1 on the image below). If you do not see it, find it using "All services" (2 on the image below).

![findactivedirectory](./Docs/findactivedirectory.png)

Click on "App registrations", and select "New registration". You will need to create two apps.

![registerappstart](./Docs/registerappstart.png)

### Register two apps

I recommend you register two apps: 
1. For the landing page, the Azure Marketplace requires SaaS app offers to have a landing page, authenticating through Azure Active Directory. Register it as described in the [documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-webapp#option-2-register-and-manually-configure-your-application-and-code-sample). **Make sure you register a multi-tenant application**, you can find the differences in the [documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/single-and-multi-tenant-apps). Set the value of the ClientId setting in the appsettings.json file in the "AzureAd" section to the AppId (clientId) for the app. 
1. To authenticate Azure Marketplace Fulfillment APIs, you register a **single tenant application**. You will need to provide the application ID (also referred to as the "client ID") and the tenant ID on the ["Technical Configuration page"](https://docs.microsoft.com/en-us/azure/marketplace/partner-center-portal/offer-creation-checklist#technical-configuration-page) on the Partner portal while registering your offer.  Set the ClientId value of the "FulfillmentClient:AzureActiveDirectory" section to this value. You will need to create a client key, and either put it in the appsettings.json file or add it as a user secret using ```dotnet user-secrets``` command. You will also need to set the TenantId as described in the appsettings.json file. Remember, this is a single tenant app.

## Creating and configuring a SendGrid account

Follow the steps in the [tutorial](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email), and grab an API Key. Set the value of the ApiKey in the configuration section, "Dashboard:Mail", either using the user-secrets method or in the appconfig.json file.

## Running the sample

You can run the sample either in Docker by building an image using the Dockerfile on the src/Dashboard folder, or running with ```dotnet run``` in the "ContosoAMPBasic\src\Dashboard" folder. Once you run the application, you need to grab the URL and update the "redirect URLs" section of the multi-tenant AD application's "Authentican" settings. E.g. if you run using ```dotnet run```, make sure to add https://localhost:5001/ and https://localhost:5001/signin-oidc to the URL list.

The default page uses the "Dashboard:DashboardAdmin" value to authorize the logged on user. Make sure to set it to your email in the configuration. The default page queries the marketplace API to list the subscriptions to the offers.

![offersubscriptions](./Docs/offersubscriptions.png)

The sample uses the mock API with its default "FulfillmentClient:FulfillmentService" settings. 

Now you want to navigate to the landing page to simulate a redirect. Append "/landingpage?token=wwww" to the URL. It should look something like this, "https://localhost:5001/landingpage?token=wwww".

Since we are using the mock API, the value of the token does not really matter.

If everything is configured correctly, you should see the landing page, be able to initiate your subscription, and receive an email momentarily after clicking the "all is good..." button.

![landingpage](./Docs/landingpage.png)

The email should look like the following.

![receivedemail](./Docs/receivedemail.png)

At this point we can assume the operations team will go off and provision the customer using the received details. The example captures the "MaximumNumberofThingsToHandle" and "Region". After the operations team complete their tasks and ready to activate the subscription, they need to come back to this email and click on the "Click here to activate subscription" link in the email.

## Simulating the cancelled subscription

I included a Postman collection containing one request to the webhook endpoint. Send the request using Postman, and go through the same steps as described above, this time for decommisioning the customer.

