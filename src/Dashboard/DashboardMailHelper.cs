using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dashboard.Controllers;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Dashboard
{
    public class DashboardMailHelper : IMailHelper
    {
        private readonly DashboardOptions options;

        public DashboardMailHelper(IOptionsMonitor<DashboardOptions> optionsMonitor)
        {
            this.options = optionsMonitor.CurrentValue;
        }

        public async Task SendActivateEmailAsync(Guid subscriptionId)
        {
            var msg = new SendGridMessage();

            msg.SetFrom(new EmailAddress(this.options.Mail.FromEmail, "Marketplace Dashboard"));

            var recipients = new List<EmailAddress>
            {
                new EmailAddress(this.options.Mail.AdminEmail)
            };
            msg.AddTos(recipients);

            msg.SetSubject("New subscription");

            msg.AddContent(MimeType.Html, $"<p>New subscription... <a href=\"{this.options.DashboardUri}\"/Activate?subscriptionId{subscriptionId}</p>");

            var client = new SendGridClient(this.options.Mail.Password);
            var response = await client.SendEmailAsync(msg);
        }
    }
}