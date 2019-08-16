namespace Dashboard.Controllers
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    using SaaSFulfillmentClient.WebHook;

    public class WebHookController : Controller
    {
        private readonly IWebhookProcessor webhookProcessor;

        private DashboardOptions options;

        public WebHookController(IWebhookProcessor webhookProcessor, IOptionsMonitor<DashboardOptions> optionsMonitor)
        {
            this.webhookProcessor = webhookProcessor;
            this.options = optionsMonitor.CurrentValue;
        }

        public async Task<IActionResult> Index(WebhookPayload payload)
        {
            // Options is injected as a singleton. This is not a good hack, but need to pass the host name and port
            this.options.BaseUrl = $"{this.Request.Scheme}://{this.Request.Host}/";
            await this.webhookProcessor.ProcessWebhookNotificationAsync(payload);
            return this.Ok();
        }
    }
}