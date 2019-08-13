using SaaSFulfillmentClient.AzureAD;
using SaaSFulfillmentClient.Models;

namespace Dashboard.Marketplace
{
    public class FulfillmentManagerOptions
    {
        public AuthenticationConfiguration AzureActiveDirectory { get; set; }

        public FulfillmentClientConfiguration FulfillmentService { get; set; }
    }
}