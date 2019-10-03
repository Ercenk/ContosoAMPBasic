namespace Dashboard.Marketplace
{
    using System.Threading;
    using System.Threading.Tasks;

    using Dashboard.Models;

    public interface IFulfillmentHandler
    {
        Task<AzureSubscriptionProvisionModel> BuildLandingPageModel(string token, CancellationToken cancellationToken);

        Task ProcessLandingPageAsync(
            AzureSubscriptionProvisionModel provisionModel,
            CancellationToken cancellationToken);
    }
}