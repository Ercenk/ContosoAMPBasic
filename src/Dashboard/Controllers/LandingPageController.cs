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
        private readonly IFulfillmentHandler fulfillmentHandler;

        private readonly ILogger<LandingPageController> logger;

        private readonly DashboardOptions options;

        public LandingPageController(
            IOptionsMonitor<DashboardOptions> dashboardOptions,
            IFulfillmentHandler fulfillmentHandler,
            IMarketplaceNotificationHandler notificationHelper,
            ILogger<LandingPageController> logger)
        {
            this.fulfillmentHandler = fulfillmentHandler;
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
                await this.fulfillmentHandler.ProcessLandingPageAsync(provisionModel, cancellationToken);

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

            var provisioningModel = await this.fulfillmentHandler.BuildLandingPageModel(token, cancellationToken);

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
    }
}