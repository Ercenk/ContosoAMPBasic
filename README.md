# ContosoAMPBasic sample for Azure Marketplace SaaS integration
This sample demonstrates the basic interaction of a SaaS solution with Azure Marketplace. It does not have any SaaS functionality, however it is a bare bones approach focusing on the marketplace integration.

Please see the related [section](https://github.com/Ercenk/AzureMarketplaceSaaSApiClient#integrating-a-software-as-a-solution-with-azure-marketplace) on my marketplace REST API client implementation for an overview of the integration concepts.  

This sample can be a good starting point if the solution does not have requirements for providing native experience for cancelling and updating a subscription.

It exposes a landing page that can be customized for branding, and including various questions. It provides a webhook endpoint for processing the incoming notifications from the Azure Marketplace. Rest of the integration is done via emails.

When a subscriber provides the details on the landing page, the solution generates an email to the configured operations contact. The operations team then provisions the required resources and onbards the customer using their internal processes then comes back to the generated email and clicks on the link in the email to activate the subscription.

The solution does not implement native cancel or update subscription options. The subscriber uses the Azure Marketplace to cancel or update the subscription. In the case of a change on the subscription (cancel, update, suspend), the marketplace posts a notification to the webhook, and the operations contact receives an email. Just like the onboarding operation, the operations contact performs the required operation manually, and then comes back to the email, and click on the included link to indicate completion of the operation.

![overview](./Docs/Overview.png)

## Notes

## Secrets

Secrets such as API keys are managed through "dotnet user-secrets" command. For example, to set the value for "FulfillmentClient:AzureActiveDirectory:AppKey" use the following command:

``` sh
dotnet user-secrets set "FulfillmentClient:AzureActiveDirectory:AppKey" "secret here"
```

Please see the user secrets [documentation](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=windows) for more details.
