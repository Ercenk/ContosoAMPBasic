using System.Threading.Tasks;
using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;

namespace Dashboard
{
    public class ContosoWebhookHandler : IWebhookHandler
    {
        public Task ChangePlanAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }

        public Task ChangeQuantityAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }

        public Task ReinstatedAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }

        public Task SubscribedAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }

        public Task SuspendedAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }

        public Task UnsubscribedAsync(WebhookPayload payload)
        {
            throw new System.NotImplementedException();
        }
    }
}