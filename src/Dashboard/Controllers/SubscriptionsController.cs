namespace Dashboard.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Dashboard.Marketplace;
    using Dashboard.Models;

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

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index()
        {
            var subscriptions = await this.fulfillmentManager.GetSubscriptionsAsync();
            return this.View(subscriptions);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotAuthorized()
        {
            return View();
        }

        public async Task<IActionResult> Operations(Guid subscriptionId)
        {
            var operations = await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId);

            return this.View(operations);
        }
    }
}