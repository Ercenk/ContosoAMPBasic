using System.Linq;
using SaaSFulfillmentClient;
using SaaSFulfillmentClient.Models;

namespace Dashboard.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Marketplace;
    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    [Authorize]
    public class LandingPageController : Controller
    {
        private readonly IFulfillmentClient fulfillmentClient;
        private readonly IMarketplaceNotificationHandler notificationHandler;
        private readonly ILogger<LandingPageController> logger;

        private readonly DashboardOptions options;

        public LandingPageController(
            IOptionsMonitor<DashboardOptions> dashboardOptions,
            IFulfillmentClient fulfillmentClient,
            IMarketplaceNotificationHandler notificationHandler,
            ILogger<LandingPageController> logger)
        {
            this.fulfillmentClient = fulfillmentClient;
            this.notificationHandler = notificationHandler;
            this.logger = logger;
            this.options = dashboardOptions.CurrentValue;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken)
        {
            var urlBase = $"{this.Request.Scheme}://{this.Request.Host}";
            this.options.BaseUrl = urlBase;
            try
            {
                await this.ProcessLandingPageAsync(provisionModel, cancellationToken);

                return this.RedirectToAction(nameof(this.Success));
            }
            catch (Exception ex)
            {
                return this.BadRequest(ex);
            }
        }

        // GET: LandingPage
        public async Task<ActionResult> Index(string token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(token))
            {
                this.ModelState.AddModelError(string.Empty, "Token URL parameter cannot be empty");
                return this.View();
            }

            var provisioningModel = await this.BuildLandingPageModel(token, cancellationToken);

            if (provisioningModel != default)
            {
                provisioningModel.FullName = (this.User.Identity as ClaimsIdentity)?.FindFirst("name")?.Value;
                provisioningModel.Email = this.User.Identity.GetUserEmail();
                provisioningModel.BusinessUnitContactEmail = this.User.Identity.GetUserEmail();

                return this.View(provisioningModel);
            }

            this.ModelState.AddModelError(string.Empty, "Cannot resolve subscription");
            return this.View();
        }

        public ActionResult Success()
        {
            return this.View();
        }

        private async Task<AzureSubscriptionProvisionModel> BuildLandingPageModel(
           string token,
           CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var resolvedSubscription = await this.fulfillmentClient.ResolveSubscriptionAsync(
                                           token,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            if (resolvedSubscription == default(ResolvedSubscription)) return default;

            var existingSubscription = await this.fulfillmentClient.GetSubscriptionAsync(
                                           resolvedSubscription.SubscriptionId,
                                           requestId,
                                           correlationId,
                                           cancellationToken);

            var availablePlans = (await this.fulfillmentClient.GetSubscriptionPlansAsync(
                                      resolvedSubscription.SubscriptionId,
                                      requestId,
                                      correlationId,
                                      cancellationToken)).Plans.ToList();

            // remove the base plan from the model to show if advanced flow is active
            if (this.options.AdvancedFlow)
            {
                if (availablePlans.Any(p => p.PlanId == this.options.BasePlanId))
                {
                    availablePlans.Remove(availablePlans.Single(p => p.PlanId == this.options.BasePlanId));
                }
            }

            var provisioningModel = new AzureSubscriptionProvisionModel
            {
                PlanId = resolvedSubscription.PlanId,
                SubscriptionId = resolvedSubscription.SubscriptionId,
                OfferId = resolvedSubscription.OfferId,
                SubscriptionName = resolvedSubscription.SubscriptionName,
                PurchaserEmail = existingSubscription.Purchaser.EmailId,
                PurchaserTenantId = existingSubscription.Purchaser.TenantId,

                // Assuming this will be set to the value the customer already set when subscribing, if we are here after the initial subscription activation
                // Landing page is used both for initial provisioning and configuration of the subscription.
                Region = TargetContosoRegionEnum.NorthAmerica,
                AvailablePlans = availablePlans,
                SubscriptionStatus = existingSubscription.SaasSubscriptionStatus,
                AdvancedFlow = this.options.AdvancedFlow,
                BasePlanId = this.options.BasePlanId,
                PendingOperations =
                                                (await this.fulfillmentClient.GetSubscriptionOperationsAsync(
                                                     resolvedSubscription.SubscriptionId,
                                                     requestId,
                                                     correlationId,
                                                     cancellationToken)).Any(
                                                    o => o.Status == OperationStatusEnum.InProgress)
            };

            return provisioningModel;
        }

        private async Task ProcessLandingPageAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken)
        {
            provisionModel.AdvancedFlow = this.options.AdvancedFlow;

            // A new subscription will have PendingFulfillmentStart as status
            if (provisionModel.SubscriptionStatus != StatusEnum.Subscribed)
            {
                if (this.options.AdvancedFlow)
                {
                    // ATTENTION:
                    // Following implementation is for demonstration purposes only and not guaranteed to work
                    // in production. The APIs are asynchronous APIs and required to poll for completion before
                    // another request is submitted. A better implementation will be to decouple this process
                    // and implement a work queue mechanism.

                    // We want to activate the subscription right away if we are in advanced flow
                    var requestId = Guid.NewGuid();
                    var correlationId = Guid.NewGuid();
                    var activateResult = await this.fulfillmentClient.ActivateSubscriptionAsync(
                                             provisionModel.SubscriptionId,
                                             new ActivatedSubscription { PlanId = provisionModel.PlanId },
                                             requestId,
                                             correlationId,
                                             cancellationToken);

                    // See if the subscriber already selected the desired plan when subscribing.
                    // Otherwise, we expect the customer to select one of the available plans.
                    if (provisionModel.PlanId != this.options.BasePlanId)
                    {
                        provisionModel.NewPlanId = provisionModel.PlanId;

                        // Update the subscription to base plan
                        var updateResult = await this.fulfillmentClient.UpdateSubscriptionPlanAsync(
                                               provisionModel.SubscriptionId,
                                               this.options.BasePlanId,
                                               requestId,
                                               correlationId,
                                               cancellationToken);
                    }

                    await this.notificationHandler.ProcessChangePlanAsync(provisionModel, cancellationToken);
                }
                else
                {
                    await this.notificationHandler.ProcessActivateAsync(provisionModel, cancellationToken);
                }
            }
            else
            {
                await this.notificationHandler.ProcessChangePlanAsync(provisionModel, cancellationToken);
            }
        }
    }
}