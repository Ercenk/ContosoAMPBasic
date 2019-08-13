using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using Dashboard.Mail;
using Dashboard.Marketplace;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dashboard.Controllers
{
    public class LandingPageController : Controller
    {
        private readonly IFulfillmentManager fulfillmentManager;
        private readonly IEMailHelper ieMailHelper;
        private readonly ILogger<LandingPageController> logger;
        private readonly DashboardOptions options;

        public LandingPageController(IOptionsMonitor<DashboardOptions> dashboardOptions, IFulfillmentManager fulfillmentManager, IEMailHelper ieMailHelper, ILogger<LandingPageController> logger)
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
            try
            {
                await this.ieMailHelper.SendActivateEmailAsync(urlBase, provisionModel);

                return this.RedirectToAction(nameof(Success));
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
                SubscriptionName = resolvedSubscription.SubscriptionName
            };

            return this.View(provisioningModel);
        }

        public ActionResult Success()
        {
            return this.View();
        }
    }
}