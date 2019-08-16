namespace Dashboard.Models
{
    using System;

    public class AzureSubscriptionProvisionModel
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public string OfferId { get; set; }

        public string PlanName { get; set; }

        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }
    }
}