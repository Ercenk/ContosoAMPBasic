namespace Dashboard.Models
{
    using System;

    public class ActivateActionViewModel
    {
        public string PlanId { get; set; }

        public Guid SubscriptionId { get; set; }
    }
}