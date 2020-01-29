namespace Dashboard.Marketplace
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using SaaSFulfillmentClient;
    using SaaSFulfillmentClient.Models;

    public class FulfillmentHandler : IFulfillmentHandler
    {
        private readonly IFulfillmentClient fulfillmentClient;

        private readonly ILogger<FulfillmentHandler> logger;

        private readonly IMarketplaceNotificationHandler notificationHandler;

        private readonly DashboardOptions options;

        public FulfillmentHandler(
            IFulfillmentClient fulfillmentClient,
            IMarketplaceNotificationHandler notificationHandler,
            IOptionsMonitor<DashboardOptions> optionsMonitor,
            ILogger<FulfillmentHandler> logger)
        {
            this.options = optionsMonitor.CurrentValue;
            this.fulfillmentClient = fulfillmentClient;
            this.notificationHandler = notificationHandler;
            this.logger = logger;
        }

        public async Task<AzureSubscriptionProvisionModel> BuildLandingPageModel(
            string token,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var resolvedSubscription = await this.fulfillmentClient.ResolveSubscriptionAsync(
                                           token,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            if (resolvedSubscription == default(ResolvedSubscription)) return default;

            var existingSubscription = await this.fulfillmentClient.GetSubscriptionAsync(
                                           resolvedSubscription.SubscriptionId,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            var availablePlans = (await this.fulfillmentClient.GetSubscriptionPlansAsync(
                                      resolvedSubscription.SubscriptionId,
                                      requestId,
                                      correlationId,
                                      cancellationToken)).Plans.ToList();

            // remove the base plan from the model to show if advanced flow is active
            if (this.options.AdvancedFlow)
            {
                if (availablePlans.Any(p => p.PlanId == this.options.BasePlanId))
                {
                    availablePlans.Remove(availablePlans.Single(p => p.PlanId == this.options.BasePlanId));
                }
            }

            var provisioningModel = new AzureSubscriptionProvisionModel
            {
                PlanId = resolvedSubscription.PlanId,
                SubscriptionId = resolvedSubscription.SubscriptionId,
                OfferId = resolvedSubscription.OfferId,
                SubscriptionName = resolvedSubscription.SubscriptionName,

                // Assuming this will be set to the value the customer already set when subscribing, if we are here after the initial subscription activation
                // Landing page is used both for initial provisioning and configuration of the subscription.
                Region = TargetContosoRegionEnum.NorthAmerica,
                AvailablePlans = availablePlans,
                SubscriptionStatus = existingSubscription.SaasSubscriptionStatus,
                AdvancedFlow = this.options.AdvancedFlow,
                BasePlanId = this.options.BasePlanId,
                PendingOperations =
                                                (await this.fulfillmentClient.GetSubscriptionOperationsAsync(
                                                     resolvedSubscription.SubscriptionId,
                                                     requestId,
                                                     correlationId,
                                                     cancellationToken)).Any(
                                                    o => o.Status == OperationStatusEnum.InProgress)
            };

            return provisioningModel;
        }

        public async Task ProcessLandingPageAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken)
        {
            provisionModel.AdvancedFlow = this.options.AdvancedFlow;

            // A new subscription will have PendingFulfillmentStart as status
            if (provisionModel.SubscriptionStatus != StatusEnum.Subscribed)
            {
                if (this.options.AdvancedFlow)
                {
                    // ATTENTION:
                    // Following implementation is for demonstration purposes only and not guaranteed to work
                    // in production. The APIs are asynchronous APIs and required to poll for completion before
                    // another request is submitted. A better implementation will be to decouple this process
                    // and implement a work queue mechanism.

                    // We want to activate the subscription right away if we are in advanced flow
                    var requestId = Guid.NewGuid();
                    var correlationId = Guid.NewGuid();
                    var activateResult = await this.fulfillmentClient.ActivateSubscriptionAsync(
                                             provisionModel.SubscriptionId,
                                             new ActivatedSubscription { PlanId = provisionModel.PlanId },
                                             requestId,
                                             correlationId,
                                             cancellationToken);

                    // See if the subscriber already selected the desired plan when subscribing.
                    // Otherwise, we expect the customer to select one of the available plans.
                    if (provisionModel.PlanId != this.options.BasePlanId)
                    {
                        provisionModel.NewPlanId = provisionModel.PlanId;

                        // Update the subscription to base plan
                        var updateResult = await this.fulfillmentClient.UpdateSubscriptionPlanAsync(
                                               provisionModel.SubscriptionId,
                                               this.options.BasePlanId,
                                               requestId,
                                               correlationId,
                                               cancellationToken);
                    }

                    await this.notificationHandler.ProcessChangePlanAsync(provisionModel, cancellationToken);
                }
                else
                {
                    await this.notificationHandler.ProcessActivateAsync(provisionModel, cancellationToken);
                }
            }
            else
            {
                await this.notificationHandler.ProcessChangePlanAsync(provisionModel, cancellationToken);
            }
        }
    }
}