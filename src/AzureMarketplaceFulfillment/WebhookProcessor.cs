using System;
using System.Threading;
using System.Threading.Tasks;
using AzureMarketplaceFulfillment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using SaaSFulfillmentClient;
using SaaSFulfillmentClient.Models;

namespace ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment
{
    public class WebhookProcessor : IWebhookProcessor
    {
        private readonly ICredentialProvider credentialProvider;

        private readonly IFulfillmentClient fulfillmentClient;

        private readonly ILogger<WebhookProcessor> logger;

        private readonly FulfillmentManagerOptions options;

        private readonly IWebhookHandler webhookHandler;

        public WebhookProcessor(IOptionsMonitor<FulfillmentManagerOptions> optionsAccessor,
            ICredentialProvider credentialProvider,
            IFulfillmentClient fulfillmentClient,
            IWebhookHandler webhookHandler,
            ILogger<WebhookProcessor> logger) : this(
            optionsAccessor,
            credentialProvider,
            fulfillmentClient,
            AdApplicationHelper.GetApplication,
            webhookHandler,
            logger)
        {
        }

        public WebhookProcessor(
            IOptionsMonitor<FulfillmentManagerOptions> optionsAccessor,
            ICredentialProvider credentialProvider,
            IFulfillmentClient fulfillmentClient,
            Func<FulfillmentManagerOptions, ICredentialProvider, IConfidentialClientApplication> adApplicationFactory,
            IWebhookHandler webhookHandler,
            ILogger<WebhookProcessor> logger)
        {
            this.options = optionsAccessor.CurrentValue;

            this.credentialProvider = credentialProvider;
            this.fulfillmentClient = fulfillmentClient;
            this.logger = logger;
            this.AdApplication = adApplicationFactory(this.options, this.credentialProvider);
            this.webhookHandler = webhookHandler;
        }

        private IConfidentialClientApplication AdApplication { get; }

        public async Task ProcessWebhookNotificationAsync(WebhookPayload payload,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var bearerToken = await AdApplicationHelper.GetBearerToken(this.AdApplication);

            // Always query the fulfillment API for the received Operation for security reasons. Webhook endpoint is not authenticated.
            var operationDetails = await this.fulfillmentClient.GetSubscriptionOperationAsync(payload.SubscriptionId,
                payload.OperationId,
                requestId,
                correlationId,
                bearerToken,
                cancellationToken);

            if (!operationDetails.Success)
            {
                this.logger.LogError(
                    $"Operation query returned {JsonConvert.SerializeObject(operationDetails)} for subscription {payload.SubscriptionId} operation {payload.OperationId}");
                return;
            }

            this.logger.LogInformation($"Received webhook notification with payload, {JsonConvert.SerializeObject(payload)}");

            switch (payload.Action)
            {
                case WebhookAction.Subscribe:
                    await this.webhookHandler.SubscribedAsync(payload);
                    break;

                case WebhookAction.Unsubscribe:
                    await this.webhookHandler.UnsubscribedAsync(payload);
                    break;

                case WebhookAction.ChangePlan:
                    await this.webhookHandler.ChangePlanAsync(payload);
                    break;

                case WebhookAction.ChangeQuantity:
                    await this.webhookHandler.ChangeQuantityAsync(payload);
                    break;

                case WebhookAction.Suspend:
                    await this.webhookHandler.SuspendedAsync(payload);
                    break;

                case WebhookAction.Reinstate:
                    await this.webhookHandler.ReinstatedAsync(payload);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}