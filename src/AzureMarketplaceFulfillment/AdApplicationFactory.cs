using System;
using System.Threading.Tasks;
using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;
using Microsoft.Identity.Client;

namespace AzureMarketplaceFulfillment
{
    public static class AdApplicationHelper
    {
        public static IConfidentialClientApplication GetApplication(FulfillmentManagerOptions options,
            ICredentialProvider credentialProvider)
        {
            if (!(credentialProvider is ClientSercretCredentialProvider secretProvider))
            {
                throw new NotImplementedException(
                    "Current implementation supports AD Applications with client secrets only.");
            }

            return ConfidentialClientApplicationBuilder
                .Create(options.AzureActiveDirectory.ClientId)
                .WithClientSecret(secretProvider.ClientSecret)
                .WithAuthority(AadAuthorityAudience.AzureAdMultipleOrgs)
                .Build();
        }

        public static async Task<string> GetBearerToken(IConfidentialClientApplication adApplication)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            return $"Bearer {(await adApplication.AcquireTokenForClient(scopes).ExecuteAsync()).AccessToken}";
        }
    }
}