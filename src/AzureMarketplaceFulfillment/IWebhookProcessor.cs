using System;
using System.Threading;
using System.Threading.Tasks;

namespace ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment
{
    public interface IWebhookProcessor
    {
        Task ProcessWebhookNotificationAsync(WebhookPayload details, CancellationToken cancellationToken = default);
    }
}
