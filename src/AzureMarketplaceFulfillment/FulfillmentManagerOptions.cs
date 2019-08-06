using SaaSFulfillmentClient.Models;

namespace AzureMarketplaceFulfillment
{
    public class FulfillmentManagerOptions
    {
        public AuthenticationConfiguration AzureActiveDirectory { get; set; }

        public FulfillmentClientConfiguration FulfillmentService { get; set; }
    }
}