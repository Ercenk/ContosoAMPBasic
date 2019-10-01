using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dashboard.Controllers
{
    using System.Security.Claims;

    using Dashboard.Marketplace;
    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Not fully implemented, it is here for a placeholder to demonstrate the idea
    /// </summary>

    [Authorize]
    public class LandingPageAdvancedController : Controller
    {
        private const string basePlanName = "basic";
        private readonly DashboardOptions dashboardOptions;

        private readonly IFulfillmentManager fulfillmentManager;

        private readonly ILogger<LandingPageAdvancedController> logger;
        private readonly IMarketplaceNotificationHandler notificationHelper;

        public LandingPageAdvancedController(IOptionsMonitor<DashboardOptions> dashboardOptions,
                                             IFulfillmentManager fulfillmentManager,
                                             IMarketplaceNotificationHandler notificationHelper,
                                             ILogger<LandingPageAdvancedController> logger)
        {
            this.dashboardOptions = dashboardOptions.CurrentValue;
            this.fulfillmentManager = fulfillmentManager;
            this.notificationHelper = notificationHelper;
            this.logger = logger;
        }

        public async Task<IActionResult> Index(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                this.ModelState.AddModelError(string.Empty, "Token URL parameter cannot be empty");
                return this.View();
            }

            var resolvedSubscription = await this.fulfillmentManager.ResolveSubscriptionAsync(token);
            if (resolvedSubscription == default(MarketplaceSubscription))
            {
                this.ModelState.AddModelError(string.Empty, "Cannot resolve subscription");
                return this.View();
            }

            var activateResult = await this.fulfillmentManager.ActivateSubscriptionAsync(
                                     resolvedSubscription.SubscriptionId,
                                     resolvedSubscription.PlanId,
                                     resolvedSubscription.Quantity);
            if (activateResult.SubscriptionId != resolvedSubscription.SubscriptionId
                || activateResult.PlanId != resolvedSubscription.PlanId)
            {
                this.ModelState.AddModelError(string.Empty, "Cannot activate subscription");
                return this.View();
            }

            var availablePlans =
                (await this.fulfillmentManager.GetSubscriptionPlansAsync(resolvedSubscription.SubscriptionId)).Plans;

            var desiredPlan = resolvedSubscription.PlanId;
            if (desiredPlan != basePlanName)
            {
                // Not the base plan, set to base plan, and save the desired plan for the future
                var updateResult = await this.fulfillmentManager.UpdateSubscriptionPlanAsync(
                                       resolvedSubscription.SubscriptionId,
                                       basePlanName);

                if (!updateResult.Succeeded)
                {
                    this.ModelState.AddModelError(string.Empty, "Cannot set to base plan");
                    return this.View();
                }
            }

            var fullName = (this.User.Identity as ClaimsIdentity)?.FindFirst("name")?.Value;
            var emailAddress = this.User.Identity.GetUserEmail();

            var provisioningModel = new AzureSubscriptionProvisionModel
            {
                FullName = fullName,
                AvailablePlans = desiredPlan == basePlanName ? availablePlans : default,
                PlanName = desiredPlan,
                SubscriptionId = resolvedSubscription.SubscriptionId,
                Email = emailAddress,
                OfferId = resolvedSubscription.OfferId,
                SubscriptionName = resolvedSubscription.SubscriptionName,
                Region = TargetContosoRegionEnum.NorthAmerica,
                MaximumNumberOfThingsToHandle = 0
            };

            return View(provisioningModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(AzureSubscriptionProvisionModel provisionModel)
        {
            var urlBase = $"{this.Request.Scheme}://{this.Request.Host}";
            this.dashboardOptions.BaseUrl = urlBase;
            try
            {
                await this.notificationHelper.ProcessStartProvisioningAsync(provisionModel);

                return this.RedirectToAction(nameof(this.Success));
            }
            catch (Exception ex)
            {
                return this.View(ex);
            }
        }

        public ActionResult Success()
        {
            return this.View();
        }
    }
}