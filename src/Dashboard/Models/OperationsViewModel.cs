namespace Dashboard.Models
{
    using System.Collections.Generic;

    using SaaSFulfillmentClient.Models;

    public class OperationsViewModel
    {
        public List<SubscriptionOperation> Operations { get; set; }

        public string SubscriptionName { get; set; }
    }
}