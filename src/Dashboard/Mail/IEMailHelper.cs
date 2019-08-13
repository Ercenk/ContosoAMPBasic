using System.Threading.Tasks;
using Dashboard.Models;
using SaaSFulfillmentClient.WebHook;

namespace Dashboard.Mail
{
    using System.Threading;

    public interface IEMailHelper
    {
        Task SendActivateEmailAsync(string urlBase, AzureSubscriptionProvisionModel provisionModel, CancellationToken cancellationToken = default);

        Task SendChangePlanEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendChangeQuantityEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendOperationFailOrConflictEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendReinstatedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendSuspendedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);

        Task SendUnsubscribedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default);
    }
}