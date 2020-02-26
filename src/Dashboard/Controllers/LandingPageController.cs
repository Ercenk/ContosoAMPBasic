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
            if (!resolvedSubscription.Success) return default;

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
            // A new subscription will have PendingFulfillmentStart as status
            if (provisionModel.SubscriptionStatus != StatusEnum.Subscribed)
            {
                
                await this.notificationHandler.ProcessActivateAsync(provisionModel, cancellationToken);
                
            }
            else
            {
                await this.notificationHandler.ProcessChangePlanAsync(provisionModel, cancellationToken);
            }
        }
    }
}
