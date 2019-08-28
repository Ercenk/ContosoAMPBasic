namespace Dashboard
{
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    using SaaSFulfillmentClient.WebHook;

    public interface IMarketplaceNotificationHandler
    {
        Task ProcessActivateAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default);

        Task ProcessChangePlanAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task ProcessChangeQuantityAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task ProcessOperationFailOrConflictAsync(
            WebhookPayload payload,
            CancellationToken cancellationToken = default);

        Task ProcessReinstatedAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task ProcessSuspendedAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task ProcessUnsubscribedAsync(WebhookPayload payload, CancellationToken cancellationToken = default);
    }
}