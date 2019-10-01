namespace Dashboard.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Marketplace;
    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using SaaSFulfillmentClient;
    using SaaSFulfillmentClient.Models;

    [Authorize(policy: "DashboardAdmin")]
    public class SubscriptionsController : Controller
    {
        private readonly IFulfillmentManager fulfillmentManager;

        private readonly IOperationsStore operationsStore;

        public SubscriptionsController(IFulfillmentManager fulfillmentManager, IOperationsStore operationsStore)
        {
            this.fulfillmentManager = fulfillmentManager;
            this.operationsStore = operationsStore;
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

            var subscriptionsViewModel = subscriptions.Select(SubscriptionViewModel.FromSubscription);
            foreach (var subscriptionViewModel in subscriptionsViewModel)
            {
                subscriptionViewModel.PendingOperations =
                    (await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionViewModel.SubscriptionId)).Any(
                        o => o.Status == OperationStatusEnum.InProgress);
            }

            return this.View(subscriptionsViewModel);
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotAuthorized()
        {
            return View();
        }

        public async Task<IActionResult> Operations(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscriptionOperations =
                await this.operationsStore.GetAllSubscriptionRecordsAsync(subscriptionId, cancellationToken);

            var operations = new List<SubscriptionOperation>();

            foreach (var operation in subscriptionOperations)
            {
                operations.Add(
                    await this.fulfillmentManager.GetSubscriptionOperationAsync(
                        subscriptionId,
                        operation.OperationId,
                        cancellationToken));
            }

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
                    var updateSubscriptionViewModel = new UpdateSubscriptionViewModel
                    {
                        SubscriptionId = subscriptionId,
                        SubscriptionName = subscription.Name,
                        CurrentPlan = subscription.PlanId,
                        AvailablePlans = availablePlans,
                        PendingOperations = (await this.fulfillmentManager.GetSubscriptionOperationsAsync(subscriptionId, cancellationToken)).Any(
                            o => o.Status == OperationStatusEnum.InProgress)
                    };

                    return this.View("UpdateSubscription", updateSubscriptionViewModel);

                case ActionsEnum.Ack:
                    break;

                case ActionsEnum.Unsubscribe:
                    var unsubscribeResult = await this.fulfillmentManager.RequestCancelSubscriptionAsync(subscriptionId);
                    return unsubscribeResult.Succeeded ? this.RedirectToAction("Index") : this.Error();

                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionAction), subscriptionAction, null);
            }

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(UpdateSubscriptionViewModel model, CancellationToken cancellationToken)
        {
            if ((await this.fulfillmentManager.GetSubscriptionOperationsAsync(model.SubscriptionId, cancellationToken))
                .Any(o => o.Status == OperationStatusEnum.InProgress)) return this.RedirectToAction("Index");
            var updateResult = await this.fulfillmentManager.UpdateSubscriptionPlanAsync(
                                   model.SubscriptionId,
                                   model.NewPlan);

            return updateResult.Succeeded ? this.RedirectToAction("Index") : this.Error();
        }
    }
}