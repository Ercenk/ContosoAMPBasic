namespace Dashboard.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Dashboard.Marketplace;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(policy: "DashboardAdmin")]
    public class SubscriptionsController : Controller
    {
        private readonly IFulfillmentManager fulfillmentManager;

        public SubscriptionsController(IFulfillmentManager fulfillmentManager)
        {
            this.fulfillmentManager = fulfillmentManager;
        }

        public async Task<IActionResult> Index()
        {
            var subscriptions = await this.fulfillmentManager.GetSubscriptionsAsync();
            return this.View(subscriptions);
        }

        public async Task<IActionResult> Operations(Guid subscriptionId)
        {
            var operations = await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId);

            return this.View(operations);
        }
    }
}