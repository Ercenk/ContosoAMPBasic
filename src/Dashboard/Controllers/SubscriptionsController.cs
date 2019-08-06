using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;
using Microsoft.AspNetCore.Mvc;
using SaaSFulfillmentClient;

namespace Dashboard.Controllers
{
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
            return View(subscriptions);
        }

        public async Task<IActionResult> Operations(Guid subscriptionId)
        {
            var operations = await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId);

            return View(operations);
        }
    }
}