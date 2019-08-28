namespace Dashboard
{
    using System.Threading.Tasks;

    using SaaSFulfillmentClient.Models;
    using SaaSFulfillmentClient.WebHook;

    public class ContosoWebhookHandler : IWebhookHandler
    {
        private readonly IMarketplaceNotificationHandler notificationHelper;

        public ContosoWebhookHandler(IMarketplaceNotificationHandler notificationHelper)
        {
            this.notificationHelper = notificationHelper;
        }

        public async Task ChangePlanAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessChangePlanAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(payload);
                    break;
            }
        }

        public async Task ChangeQuantityAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessChangeQuantityAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(payload);
                    break;
            }
        }

        public async Task ReinstatedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessReinstatedAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(payload);
                    break;
            }
        }

        public async Task SuspendedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessSuspendedAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(payload);
                    break;
            }
        }

        public async Task UnsubscribedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessUnsubscribedAsync(payload);
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(payload);
                    break;
            }
        }
    }
}