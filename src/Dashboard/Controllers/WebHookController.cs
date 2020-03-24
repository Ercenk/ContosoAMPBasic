namespace Dashboard.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Newtonsoft.Json;

    using SaaSFulfillmentClient.WebHook;

    // Removing the authorize attribute. This will be relevant once we start receivint JWT from the marketplac engine
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [RequireHttps]
    public class WebHookController : Controller
    {
        private readonly ILogger<WebHookController> logger;

        private readonly IWebhookProcessor webhookProcessor;

        private readonly DashboardOptions options;

        public WebHookController(
            IWebhookProcessor webhookProcessor,
            IOptionsMonitor<DashboardOptions> optionsMonitor,
            ILogger<WebHookController> logger)
        {
            this.webhookProcessor = webhookProcessor;
            this.logger = logger;
            this.options = optionsMonitor.CurrentValue;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] WebhookPayload payload)
        {
            // Options is injected as a singleton. This is not a good hack, but need to pass the host name and port
            this.options.BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}/";
            await this.webhookProcessor.ProcessWebhookNotificationAsync(payload);
            this.logger.LogInformation($"Received webhook request: {JsonConvert.SerializeObject(payload)}");
            return this.Ok();
        }
    }
}