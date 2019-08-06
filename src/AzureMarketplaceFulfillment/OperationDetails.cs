using System;

namespace ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment
{
    public class OperationDetails
    {
        public Guid OperationId { get; set; }

        public Guid SubscriptionId { get; set; }

        public TimeSpan RetryInterval { get; set; }
    }
}
