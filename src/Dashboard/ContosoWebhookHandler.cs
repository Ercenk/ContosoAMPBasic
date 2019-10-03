namespace Dashboard
{
    using System;
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
                    await this.notificationHelper.NotifyChangePlanAsync(NotificationModel.FromWebhookPayload(payload));
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;
            }
        }

        public async Task ChangeQuantityAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessChangeQuantityAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;
            }
        }

        public async Task ReinstatedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessReinstatedAsync(NotificationModel.FromWebhookPayload(payload));
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;
            }
        }

        public Task SubscribedAsync(WebhookPayload payload)
        {
            throw new NotImplementedException();
        }

        public async Task SuspendedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessSuspendedAsync(NotificationModel.FromWebhookPayload(payload));
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;
            }
        }

        public async Task UnsubscribedAsync(WebhookPayload payload)
        {
            switch (payload.Status)
            {
                case OperationStatusEnum.Succeeded:
                    await this.notificationHelper.ProcessUnsubscribedAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;

                case OperationStatusEnum.Failed:
                case OperationStatusEnum.Conflict:
                    await this.notificationHelper.ProcessOperationFailOrConflictAsync(
                        NotificationModel.FromWebhookPayload(payload));
                    break;
            }
        }
    }
}