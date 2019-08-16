namespace Dashboard.Marketplace
{
    using System;

    public class OperationDetails
    {
        public Guid OperationId { get; set; }

        public TimeSpan RetryInterval { get; set; }

        public Guid SubscriptionId { get; set; }
    }
}