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

        Task ProcessChangePlanAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        Task ProcessChangeQuantityAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        Task ProcessOperationFailOrConflictAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default);

        Task ProcessReinstatedAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        Task ProcessStartProvisioningAsync(AzureSubscriptionProvisionModel provisionModel, CancellationToken cancellationToken = default);

        Task ProcessSuspendedAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        Task ProcessUnsubscribedAsync(NotificationModel notificationModel, CancellationToken cancellationToken = default);

        Task ProcessChangePlanAsync(AzureSubscriptionProvisionModel provisionModel, CancellationToken cancellationToken = default);
    }
}