namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;

    using SaaSFulfillmentClient.Models;

    public class UpdateSubscriptionViewModel
    {
        public IEnumerable<Plan> AvailablePlans { get; set; }

        public string CurrentPlan { get; set; }

        public string NewPlan { get; set; }

        public bool PendingOperations { get; set; }

        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }
    }
}