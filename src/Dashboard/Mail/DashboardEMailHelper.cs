using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SaaSFulfillmentClient;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Dashboard.Mail
{
    using System.Threading;

    using SaaSFulfillmentClient.WebHook;

    public class DashboardEMailHelper : IEMailHelper
    {
        private readonly IFulfillmentClient fulfillmentClient;

        private readonly DashboardOptions options;

        public DashboardEMailHelper(IOptionsMonitor<DashboardOptions> optionsMonitor, IFulfillmentClient fulfillmentClient)
        {
            this.fulfillmentClient = fulfillmentClient;
            this.options = optionsMonitor.CurrentValue;
        }

        public async Task SendActivateEmailAsync(string urlBase, AzureSubscriptionProvisionModel provisionModel, CancellationToken cancellationToken = default)
        {
            var queryParams = new List<Tuple<string, string>> { new Tuple<string, string>("subscriptionId", provisionModel.SubscriptionId.ToString()) };
            await this.SendEmailAsync(
                () => $"New subscription, {provisionModel.SubscriptionName}",
                () =>
                    $"<p>New subscription... {BuildALink(urlBase, "MailLink", queryParams, "Click here to activate subscription")}</p>. " +
                    $"<p> Details are {JsonConvert.SerializeObject(provisionModel, Formatting.Indented)}</p>");
        }

        public async Task SendChangePlanEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            var queryParams = new List<Tuple<string, string>> { new Tuple<string, string>("subscriptionId", payload.SubscriptionId.ToString()) };

            var subscriptionDetails = await this.fulfillmentClient.GetSubscriptionAsync(
                payload.SubscriptionId,
                Guid.Empty,
                Guid.Empty,
                cancellationToken);

            await this.SendEmailAsync(
                () => $"New subscription, {subscriptionDetails.Name}",
                () =>
                    $"<p>New subscription... "
                    //$"{BuildALink(urlBase, "MailLink", queryParams, "Click here to activate subscription")}</p>. " +
                    //$"<p> Details are {JsonConvert.SerializeObject(provisionModel, Formatting.Indented)}</p>"
                    );
        }

        public Task SendChangeQuantityEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendOperationFailOrConflictEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendReinstatedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendSuspendedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SendUnsubscribedEmailAsync(WebhookPayload payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private static string BuildALink(string urlBase, string path, IEnumerable<Tuple<string, string>> queryParams, string innerText)
        {
            var uriStart = FluentUriBuilder.Start(urlBase)
                .AddPath(path);

            foreach (var (item1, item2) in queryParams)
            {
                uriStart.AddQuery(item1, item2);
            }

            var href = uriStart.Uri.ToString();

            return $"<a href=\"{href}\">{innerText}</a>";
        }

        private async Task SendEmailAsync(Func<string> subjectBuilder, Func<string> contentBuilder, CancellationToken cancellationToken = default)
        {
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(this.options.Mail.FromEmail, "Marketplace Dashboard"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress(this.options.Mail.AdminEmail)
            };

            msg.AddTos(recipients);

            msg.SetSubject(subjectBuilder());

            msg.AddContent(MimeType.Html, contentBuilder());

            var client = new SendGridClient(this.options.Mail.ApiKey);
            var response = await client.SendEmailAsync(msg, cancellationToken);
        }
    }
}