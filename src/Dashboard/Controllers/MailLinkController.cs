namespace Dashboard.Controllers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using SaaSFulfillmentClient;
    using SaaSFulfillmentClient.Models;
    using SaaSFulfillmentClient.WebHook;

    [Authorize(policy: "DashboardAdmin")]
    public class MailLinkController : Controller
    {
        private readonly IFulfillmentClient fulfillmentClient;

        public MailLinkController(IFulfillmentClient fulfillmentClient)
        {
            this.fulfillmentClient = fulfillmentClient;
        }

        [HttpGet]
        public async Task<IActionResult> Activate(Guid subscriptionId, string planId)
        {
            var result = await this.fulfillmentClient.ActivateSubscriptionAsync(
                             subscriptionId,
                             new ActivatedSubscription() { PlanId = planId },
                             Guid.Empty,
                             Guid.Empty,
                             CancellationToken.None);

            return result.Success
                       ? this.View(new ActivateActionViewModel { SubscriptionId = subscriptionId, PlanId = planId })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        [HttpGet]
        public async Task<IActionResult> CancelSubscription(WebhookPayload payload, CancellationToken cancellationToken)
        {
            var result = await this.UpdateOperationAsync(payload, cancellationToken);

            return result.Success
                       ? this.View(
                           "OperationUpdate",
                           new OperationUpdateViewModel { OperationType = "CancelSubscription", Payload = payload })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        [HttpGet]
        public async Task<IActionResult> PlanChange(WebhookPayload payload, CancellationToken cancellationToken)
        {
            var result = await this.UpdateOperationAsync(payload, cancellationToken);

            return result.Success
                       ? this.View(
                           "OperationUpdate",
                           new OperationUpdateViewModel { OperationType = "PlanChange", Payload = payload })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        [HttpGet]
        public async Task<IActionResult> QuantityChange(WebhookPayload payload, CancellationToken cancellationToken)
        {
            var result = await this.UpdateOperationAsync(payload, cancellationToken);

            return result.Success
                       ? this.View(
                           "OperationUpdate",
                           new OperationUpdateViewModel { OperationType = "QuantityChange", Payload = payload })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        [HttpGet]
        public async Task<IActionResult> Reinstate(WebhookPayload payload, CancellationToken cancellationToken)
        {
            var result = await this.UpdateOperationAsync(payload, cancellationToken);

            return result.Success
                       ? this.View(
                           "OperationUpdate",
                           new OperationUpdateViewModel { OperationType = "Reinstate", Payload = payload })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        [HttpGet]
        public async Task<IActionResult> SuspendSubscription(
            WebhookPayload payload,
            CancellationToken cancellationToken)
        {
            var result = await this.UpdateOperationAsync(payload, cancellationToken);

            return result.Success
                       ? this.View(
                           "OperationUpdate",
                           new OperationUpdateViewModel { OperationType = "SuspendSubscription", Payload = payload })
                       : this.View(viewName: "MailActionError", FulfillmentRequestErrorViewModel.From(result));
        }

        private async Task<FulfillmentRequestResult> UpdateOperationAsync(
            WebhookPayload payload,
            CancellationToken cancellationToken)
        {
            return await this.fulfillmentClient.UpdateSubscriptionOperationAsync(
                       payload.SubscriptionId,
                       payload.OperationId,
                       new OperationUpdate
                       {
                           PlanId = payload.PlanId,
                           Quantity = payload.Quantity,
                           Status = OperationUpdateStatusEnum.Success
                       },
                       Guid.Empty,
                       Guid.Empty,
                       cancellationToken);
        }
    }
}