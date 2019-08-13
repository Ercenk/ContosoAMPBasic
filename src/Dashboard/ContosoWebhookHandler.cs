using System.Threading.Tasks;
using Dashboard.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using SaaSFulfillmentClient.Models;
using SaaSFulfillmentClient.WebHook;

namespace Dashboard
{
    public class ContosoWebhookHandler : IWebhookHandler
    {
        private readonly IEMailHelper emailHelper;

        public ContosoWebhookHandler(IEMailHelper emailHelper)
        {
            this.emailHelper = emailHelper;
        }

        public async Task ChangePlanAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.emailHelper.SendChangePlanEmailAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.emailHelper.SendOperationFailOrConflictEmailAsync(payload);
                    break;
            }
        }

        public async Task ChangeQuantityAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.emailHelper.SendChangeQuantityEmailAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.emailHelper.SendOperationFailOrConflictEmailAsync(payload);
                    break;
            }
        }

        public async Task ReinstatedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.emailHelper.SendReinstatedEmailAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.emailHelper.SendOperationFailOrConflictEmailAsync(payload);
                    break;
            }
        }

        public async Task SuspendedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.emailHelper.SendSuspendedEmailAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.emailHelper.SendOperationFailOrConflictEmailAsync(payload);
                    break;
            }
        }

        public async Task UnsubscribedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.emailHelper.SendUnsubscribedEmailAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.emailHelper.SendOperationFailOrConflictEmailAsync(payload);
                    break;
            }
        }
    }
}