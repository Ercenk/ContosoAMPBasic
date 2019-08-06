using System.Threading.Tasks;

namespace ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment
{
    public interface IWebhookHandler
    {
        Task ChangePlanAsync(WebhookPayload payload);

        Task ChangeQuantityAsync(WebhookPayload payload);

        Task ReinstatedAsync(WebhookPayload payload);

        Task SubscribedAsync(WebhookPayload payload);

        Task SuspendedAsync(WebhookPayload payload);

        Task UnsubscribedAsync(WebhookPayload payload);
    }
}
