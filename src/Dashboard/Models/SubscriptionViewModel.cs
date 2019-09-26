namespace Dashboard.Models
{
    using System;
    using System.Collections.Generic;

    using Dashboard.Marketplace;

    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using SaaSFulfillmentClient.Models;

    public class SubscriptionViewModel : MarketplaceSubscription
    {
        public IEnumerable<ActionsEnum> NextActions
        {
            get
            {
                switch (this.State)
                {
                    case StatusEnum.Provisioning:
                        return new List<ActionsEnum> { ActionsEnum.Activate };

                    case StatusEnum.Subscribed:
                        return new List<ActionsEnum> { ActionsEnum.Update, ActionsEnum.Unsubscribe };

                    case StatusEnum.Suspended:
                        return new List<ActionsEnum>();

                    case StatusEnum.Unsubscribed:
                        return new List<ActionsEnum>();

                    case StatusEnum.NotStarted:
                        break;

                    case StatusEnum.PendingFulfillmentStart:
                        break;

                    default:
                        return new List<ActionsEnum>();
                }

                return new List<ActionsEnum>();
            }
        }

        public bool PendingOperations { get; set; }

        public static SubscriptionViewModel FromSubscription(MarketplaceSubscription marketplaceSubscription)
        {
            return new SubscriptionViewModel
            {
                PlanId = marketplaceSubscription.PlanId,
                Quantity = marketplaceSubscription.Quantity,
                SubscriptionId = marketplaceSubscription.SubscriptionId,
                OfferId = marketplaceSubscription.OfferId,
                State = marketplaceSubscription.State,
                SubscriptionName = marketplaceSubscription.SubscriptionName
            };
        }
    }
}