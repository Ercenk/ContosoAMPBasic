namespace ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment
{
    public enum WebhookAction
    {
        //(When the resource has been activated)
        Subscribe,

        // (When the resource has been deleted)
        Unsubscribe,

        // (When the change plan operation has completed)
        ChangePlan,

        // (When the change quantity operation has completed),
        ChangeQuantity,

        //(When resource has been suspended)
        Suspend,

        // (When resource has been reinstated after suspension)
        Reinstate
    }
}
