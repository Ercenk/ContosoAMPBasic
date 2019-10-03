namespace Dashboard.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using SaaSFulfillmentClient;
    using SaaSFulfillmentClient.Models;

    [Authorize("DashboardAdmin")]
    public class SubscriptionsController : Controller
    {
        private readonly IFulfillmentClient fulfillmentClient;

        private readonly IOperationsStore operationsStore;

        private readonly DashboardOptions options;

        public SubscriptionsController(
            IFulfillmentClient fulfillmentClient,
            IOperationsStore operationsStore,
            IOptionsMonitor<DashboardOptions> options)
        {
            this.fulfillmentClient = fulfillmentClient;
            this.operationsStore = operationsStore;
            this.options = options.CurrentValue;
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var subscriptions = await this.fulfillmentClient.GetSubscriptionsAsync(
                                    requestId,
                                    correlationId,
                                    cancellationToken);

            var subscriptionsViewModel = subscriptions.Select(SubscriptionViewModel.FromSubscription)
                .Where(s => s.State != StatusEnum.Unsubscribed || this.options.ShowUnsubscribed);
            foreach (var subscription in subscriptionsViewModel)
            {
                subscription.PendingOperations =
                    (await this.fulfillmentClient.GetSubscriptionOperationsAsync(
                         requestId,
                         correlationId,
                         subscription.SubscriptionId,
                         cancellationToken)).Any(o => o.Status == OperationStatusEnum.InProgress);

                var recordedSubscriptionOperations =
                    await this.operationsStore.GetAllSubscriptionRecordsAsync(
                        subscription.SubscriptionId,
                        cancellationToken);

                var subscriptionOperations = new List<SubscriptionOperation>();
                foreach (var operation in recordedSubscriptionOperations)
                    subscriptionOperations.Add(
                        await this.fulfillmentClient.GetSubscriptionOperationAsync(
                            operation.SubscriptionId,
                            operation.OperationId,
                            requestId,
                            correlationId,
                            cancellationToken));

                subscription.PendingOperations |=
                    subscriptionOperations.Any(o => o.Status == OperationStatusEnum.InProgress);
            }

            return this.View(subscriptionsViewModel.OrderByDescending(s => s.SubscriptionName));
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult NotAuthorized()
        {
            return this.View();
        }

        public async Task<IActionResult> Operations(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var subscriptionOperations =
                await this.operationsStore.GetAllSubscriptionRecordsAsync(subscriptionId, cancellationToken);

            var subscription = await this.fulfillmentClient.GetSubscriptionAsync(
                                   subscriptionId,
                                   requestId,
                                   correlationId,
                                   cancellationToken);

            var operations = new List<SubscriptionOperation>();

            foreach (var operation in subscriptionOperations)
                operations.Add(
                    await this.fulfillmentClient.GetSubscriptionOperationAsync(
                        subscriptionId,
                        operation.OperationId,
                        requestId,
                        correlationId,
                        cancellationToken));

            return this.View(new OperationsViewModel { SubscriptionName = subscription.Name, Operations = operations });
        }

        public async Task<IActionResult> SubscriptionAction(
            Guid subscriptionId,
            ActionsEnum subscriptionAction,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            switch (subscriptionAction)
            {
                case ActionsEnum.Activate:
                    break;

                case ActionsEnum.Update:
                    var availablePlans = (await this.fulfillmentClient.GetSubscriptionPlansAsync(
                                              subscriptionId,
                                              requestId,
                                              correlationId,
                                              cancellationToken)).Plans.ToList();

                    // remove the base plan from the model to show
                    availablePlans.Remove(availablePlans.Single(p => p.PlanId == this.options.BasePlanId));

                    var subscription = await this.fulfillmentClient.GetSubscriptionAsync(
                                           subscriptionId,
                                           requestId,
                                           correlationId,
                                           cancellationToken);
                    var updateSubscriptionViewModel = new UpdateSubscriptionViewModel
                    {
                        SubscriptionId = subscriptionId,
                        SubscriptionName = subscription.Name,
                        CurrentPlan = subscription.PlanId,
                        AvailablePlans = availablePlans,
                        PendingOperations =
                                                                  (await this.fulfillmentClient
                                                                       .GetSubscriptionOperationsAsync(
                                                                           subscriptionId,
                                                                           requestId,
                                                                           correlationId,
                                                                           cancellationToken)).Any(
                                                                      o => o.Status == OperationStatusEnum.InProgress)
                    };

                    return this.View("UpdateSubscription", updateSubscriptionViewModel);

                case ActionsEnum.Ack:
                    break;

                case ActionsEnum.Unsubscribe:
                    var unsubscribeResult = await this.fulfillmentClient.DeleteSubscriptionAsync(
                                                subscriptionId,
                                                requestId,
                                                correlationId,
                                                cancellationToken);

                    return unsubscribeResult.Success ? this.RedirectToAction("Index") : this.Error();

                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionAction), subscriptionAction, null);
            }

            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(
            UpdateSubscriptionViewModel model,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            if ((await this.fulfillmentClient.GetSubscriptionOperationsAsync(
                     model.SubscriptionId,
                     requestId,
                     correlationId,
                     cancellationToken))
                .Any(o => o.Status == OperationStatusEnum.InProgress)) return this.RedirectToAction("Index");
            var updateResult = await this.fulfillmentClient.UpdateSubscriptionPlanAsync(
                                   model.SubscriptionId,
                                   model.NewPlan,
                                   requestId,
                                   correlationId,
                                   cancellationToken);

            return updateResult.Success ? this.RedirectToAction("Index") : this.Error();
        }
    }
}