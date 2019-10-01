namespace Dashboard.Marketplace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Logging;

    using SaaSFulfillmentClient;
    using SaaSFulfillmentClient.Models;

    public class FulfillmentManager : IFulfillmentManager
    {
        private readonly IFulfillmentClient fulfillmentClient;

        private readonly ILogger<FulfillmentManager> logger;

        public FulfillmentManager(IFulfillmentClient fulfillmentClient, ILogger<FulfillmentManager> logger)
        {
            this.fulfillmentClient = fulfillmentClient;
            this.logger = logger;
        }

        public async Task<MarketplaceSubscription> ActivateSubscriptionAsync(
            Guid subscriptionId,
            string planId,
            int? quantity,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var subscriptionToBeActivated = new ActivatedSubscription { PlanId = planId };
            if (quantity.HasValue && quantity.Value != 0)
            {
                subscriptionToBeActivated.Quantity = quantity.Value.ToString();
            }

            var result = await this.fulfillmentClient.ActivateSubscriptionAsync(
                             subscriptionId,
                             subscriptionToBeActivated,
                             requestId,
                             correlationId,
                             cancellationToken);

            if (result.Success)
            {
                this.logger.LogInformation(
                    $"Activated subscription {subscriptionId} for plan {planId} with quantitiy {quantity}");
                var returnValue = new MarketplaceSubscription
                {
                    PlanId = planId,
                    State = StatusEnum.Subscribed,
                    SubscriptionId = subscriptionId
                };

                if (quantity.HasValue)
                {
                    returnValue.Quantity = quantity.Value;
                }

                return returnValue;
            }

            this.logger.LogError(
                $"Cannot activate subscription {subscriptionId} for plan {planId} with quantitiy {quantity}");

            return default;
        }

        public async Task<Subscription> GetsubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            return await this.fulfillmentClient.GetSubscriptionAsync(subscriptionId, requestId, correlationId, cancellationToken);
        }

        public async Task<SubscriptionOperation> GetSubscriptionOperationAsync(
                    Guid receivedSubscriptionId,
            Guid operationId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var operationResult = await this.fulfillmentClient.GetSubscriptionOperationAsync(
                                      receivedSubscriptionId,
                                      operationId,
                                      requestId,
                                      correlationId,
                                      cancellationToken);

            return operationResult;
        }

        public async Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var operations = await this.fulfillmentClient.GetSubscriptionOperationsAsync(
                                 subscriptionId,
                                 requestId,
                                 correlationId,
                                 cancellationToken);

            return operations;
        }

        public async Task<SubscriptionPlans> GetSubscriptionPlansAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            return await this.fulfillmentClient.GetSubscriptionPlansAsync(
                       subscriptionId,
                       requestId,
                       correlationId,
                       cancellationToken);
        }

        public async Task<IEnumerable<MarketplaceSubscription>> GetSubscriptionsAsync(
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var subscriptions = await this.fulfillmentClient.GetSubscriptionsAsync(
                                    requestId,
                                    correlationId,
                                    cancellationToken);

            return subscriptions.Select(s => MarketplaceSubscription.From(s));
        }

        public async Task<FulfillmentManagerOperationResult> RequestCancelSubscriptionAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var deleteRequest = await this.fulfillmentClient.DeleteSubscriptionAsync(
                                    subscriptionId,
                                    requestId,
                                    correlationId,
                                    cancellationToken);

            if (!deleteRequest.Success)
            {
                return FulfillmentManagerOperationResult.Failed(new FulfillmentManagementError { Description = string.Empty });
            }

            return FulfillmentManagerOperationResult.Success;
        }

        public async Task<MarketplaceSubscription> ResolveSubscriptionAsync(
            string authCode,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var subscription = await this.fulfillmentClient.ResolveSubscriptionAsync(
                                   authCode,
                                   requestId,
                                   correlationId,
                                   cancellationToken);

            if (subscription.Success)
            {
                this.logger.LogInformation(
                    $"Resolved subscription {subscription.SubscriptionId} for plan {subscription.PlanId} with quantitiy {subscription.Quantity}");
                return MarketplaceSubscription.From(subscription, StatusEnum.Provisioning);
            }

            this.logger.LogError("Cannot resolve subscription.");
            return default;
        }

        public async Task<FulfillmentManagerOperationResult> UpdateSubscriptionPlanAsync(
            Guid subscriptionId,
            string planId,
            CancellationToken cancellationToken = default)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var result = await this.fulfillmentClient.UpdateSubscriptionPlanAsync(
                             subscriptionId,
                             planId,
                             requestId,
                             correlationId,
                             cancellationToken);

            if (!result.Success)
            {
                return FulfillmentManagerOperationResult.Failed(
                    new FulfillmentManagementError { Description = result.RawResponse });
            }

            var operation = await this.fulfillmentClient.GetSubscriptionOperationAsync(
                                subscriptionId,
                                result.OperationId,
                                requestId,
                                correlationId,
                                cancellationToken);

            var returnResult = FulfillmentManagerOperationResult.Success;
            returnResult.Operation = operation;

            return returnResult;
        }
    }
}