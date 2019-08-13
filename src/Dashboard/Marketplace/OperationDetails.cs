using System;

namespace Dashboard.Marketplace
{
    public class OperationDetails
    {
        public Guid OperationId { get; set; }

        public Guid SubscriptionId { get; set; }

        public TimeSpan RetryInterval { get; set; }
    }
}
