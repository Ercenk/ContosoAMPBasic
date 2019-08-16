namespace Dashboard.Models
{
    using SaaSFulfillmentClient.WebHook;

    public class OperationUpdateViewModel
    {
        public string OperationType { get; set; }

        public WebhookPayload Payload { get; set; }
    }
}