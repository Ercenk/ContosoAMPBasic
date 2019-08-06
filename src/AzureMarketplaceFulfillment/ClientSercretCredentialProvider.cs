using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;

namespace AzureMarketplaceFulfillment
{
    internal class ClientSercretCredentialProvider : ICredentialProvider
    {
        public ClientSercretCredentialProvider(string clientSecret)
        {
            ClientSecret = clientSecret;
        }

        public string ClientSecret { get; }
    }
}