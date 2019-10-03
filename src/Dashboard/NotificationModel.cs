namespace Dashboard
{
    using System;

    using SaaSFulfillmentClient.WebHook;

    public class NotificationModel
    {
        public string OfferId { get; set; }

        public Guid OperationId { get; set; }

        public string OperationType { get; set; }

        public string PlanId { get; set; }

        public string PublisherId { get; set; }

        public int Quantity { get; set; }

        public Guid SubscriptionId { get; set; }

        public static NotificationModel FromWebhookPayload(WebhookPayload payload)
        {
            return new NotificationModel
                       {
                           OfferId = payload.OfferId,
                           OperationId = payload.OperationId,
                           PlanId = payload.PlanId,
                           PublisherId = payload.PublisherId,
                           Quantity = payload.Quantity,
                           SubscriptionId = payload.SubscriptionId
                       };
        }
    }
}