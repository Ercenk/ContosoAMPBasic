namespace Dashboard.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using Dashboard.Mail;
    using Dashboard.Marketplace;
    using Dashboard.Models;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    [Authorize]
    public class LandingPageController : Controller
    {
        private readonly IFulfillmentManager fulfillmentManager;

        private readonly IEMailHelper ieMailHelper;

        private readonly ILogger<LandingPageController> logger;

        private readonly DashboardOptions options;

        public LandingPageController(
            IOptionsMonitor<DashboardOptions> dashboardOptions,
            IFulfillmentManager fulfillmentManager,
            IEMailHelper ieMailHelper,
            ILogger<LandingPageController> logger)
        {
            this.fulfillmentManager = fulfillmentManager;
            this.ieMailHelper = ieMailHelper;
            this.logger = logger;
            this.options = dashboardOptions.CurrentValue;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(AzureSubscriptionProvisionModel provisionModel)
        {
            var urlBase = $"{this.Request.Scheme}://{this.Request.Host}";
            this.options.BaseUrl = urlBase;
            try
            {
                await this.ieMailHelper.SendActivateEmailAsync(provisionModel);

                return this.RedirectToAction(nameof(this.Success));
            }
            catch (Exception ex)
            {
                return this.View(ex);
            }
        }

        // GET: LandingPage
        public async Task<ActionResult> Index(string token)
        {
            var resolvedSubscription = await this.fulfillmentManager.ResolveSubscriptionAsync(token);
            if (resolvedSubscription == default(MarketplaceSubscription))
            {
                this.ModelState.AddModelError(string.Empty, "Cannot resolve subscription");
                return this.View();
            }

            var fullName = (this.User.Identity as ClaimsIdentity)?.FindFirst("name")?.Value;
            var emailAddress = (this.User.Identity as ClaimsIdentity)?.FindFirst("preferred_username")?.Value;

            var provisioningModel = new AzureSubscriptionProvisionModel
            {
                FullName = fullName,
                PlanName = resolvedSubscription.PlanId,
                SubscriptionId = resolvedSubscription.SubscriptionId,
                Email = emailAddress,
                OfferId = resolvedSubscription.OfferId,
                SubscriptionName = resolvedSubscription.SubscriptionName,
                Region = TargetContosoRegionEnum.NorthAmerica,
                MaximumNumberOfThingsToHandle = 0
            };

            return this.View(provisioningModel);
        }

        public ActionResult Success()
        {
            return this.View();
        }
    }
}