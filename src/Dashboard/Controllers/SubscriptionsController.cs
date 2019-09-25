namespace Dashboard.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
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
            var subscriptionsViewModel = subscriptions.Select(s => SubscriptionViewModel.FromSubscription(s));
            return this.View(subscriptionsViewModel);
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

        public async Task<IActionResult> SubscriptionAction(Guid subscriptionId, ActionsEnum subscriptionAction, CancellationToken cancellationToken)
        {
            switch (subscriptionAction)
            {
                case ActionsEnum.Activate:
                    break;

                case ActionsEnum.Update:
                    var availablePlans = 
                                (await this.fulfillmentManager.GetSubscriptionPlansAsync(subscriptionId, cancellationToken)).Plans;
                    var subscription = (await this.fulfillmentManager.GetsubscriptionAsync(subscriptionId, cancellationToken));
                    var updateSubscriptionViewModel = new  UpdateSubscriptionViewModel {
                        CurrentPlan = subscription.PlanId,
                        AvailablePlans = availablePlans};

                    return this.View("UpdateSubscription", updateSubscriptionViewModel);

                case ActionsEnum.Ack:
                    break;

                case ActionsEnum.Unsubscribe:
                    await this.fulfillmentManager.RequestCancelSubscriptionAsync(subscriptionId);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionAction), subscriptionAction, null);
            }

            var operations = await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId);

            return this.View();
        }
    }
}