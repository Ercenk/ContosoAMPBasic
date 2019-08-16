namespace Dashboard.Marketplace
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using SaaSFulfillmentClient.Models;

    public interface IFulfillmentManager
    {
        Task<MarketplaceSubscription> ActivateSubscriptionAsync(
            Guid subscriptionId,
            string planId,
            int? quantity,
            CancellationToken cancellationToken = default);

        Task<FulfillmentManagerOperationResult> GetOperationResultAsync(
            Guid receivedSubscriptionId,
            Guid operationId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<SubscriptionOperation>> GetSubscriptionOperationsAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<MarketplaceSubscription>> GetSubscriptionsAsync(CancellationToken cancellationToken = default);

        Task<FulfillmentManagerOperationResult> RequestCancelSubscriptionAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default);

        Task<FulfillmentManagerOperationResult> RequestUpdateSubscriptionAsync(
            Guid subscriptionId,
            string name,
            CancellationToken cancellationToken = default);

        Task<MarketplaceSubscription> ResolveSubscriptionAsync(
            string authCode,
            CancellationToken cancellationToken = default);
    }
}