namespace Dashboard.Marketplace
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using SaaSFulfillmentClient.Models;

    public class MarketplaceSubscription
    {
        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public int Quantity { get; set; }

        public StatusEnum State { get; set; }

        public Guid SubscriptionId { get; set; }

        [Display(Name = "Name")]
        public string SubscriptionName { get; set; }

        internal static MarketplaceSubscription From(ResolvedSubscription subscription, StatusEnum newState)
        {
            return new MarketplaceSubscription
            {
                SubscriptionId = subscription.SubscriptionId,
                OfferId = subscription.OfferId,
                PlanId = subscription.PlanId,
                Quantity = subscription.Quantity,
                SubscriptionName = subscription.SubscriptionName,
                State = newState
            };
        }

        internal static MarketplaceSubscription From(Subscription subscription)
        {
            return new MarketplaceSubscription
            {
                SubscriptionId = subscription.SubscriptionId,
                OfferId = subscription.OfferId,
                PlanId = subscription.PlanId,
                Quantity = subscription.Quantity,
                SubscriptionName = subscription.Name,
                State = subscription.SaasSubscriptionStatus
            };
        }
    }
}