namespace Dashboard.Marketplace
{
    using SaaSFulfillmentClient.AzureAD;
    using SaaSFulfillmentClient.Models;

    public class FulfillmentManagerOptions
    {
        public AuthenticationConfiguration AzureActiveDirectory { get; set; }

        public FulfillmentClientConfiguration FulfillmentService { get; set; }
    }
}