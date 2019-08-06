using System.Security.Cryptography.X509Certificates;
using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;

namespace AzureMarketplaceFulfillment
{
    public class CertificateCredentialProvider : ICredentialProvider
    {
        public StoreName CertificateStore { get; set; }
        public string CertificateThumprint { get; set; }
        public StoreLocation StoreLocation { get; set; }
    }
}