namespace Dashboard.Mail
{
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    using SaaSFulfillmentClient.WebHook;

    public interface IEMailHelper
    {
        Task SendActivateEmailAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken = default);

        Task SendChangePlanEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendChangeQuantityEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendOperationFailOrConflictEmailAsync(
            WebhookPayload payload,
            CancellationToken cancellationToken = default);

        Task SendReinstatedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendSuspendedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendUnsubscribedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);
    }
}