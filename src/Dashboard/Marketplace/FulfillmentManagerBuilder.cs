namespace Dashboard.Marketplace
{
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using SaaSFulfillmentClient.AzureAD;

    public class FulfillmentManagerBuilder
    {
        private readonly IServiceCollection services;

        public FulfillmentManagerBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public FulfillmentManagerOptions Options { get; }

        public void WithCertificateAuthentication(
            StoreLocation storeLocation,
            StoreName storeName,
            string certificateThumbprint)
        {
            this.services.TryAddSingleton<ICredentialProvider>(
                new CertificateCredentialProvider
                    {
                        StoreLocation = storeLocation,
                        CertificateStore = storeName,
                        CertificateThumprint = certificateThumbprint
                    });
        }

        public void WithClientSecretAuthentication(string clientSecret)
        {
            this.services.TryAddSingleton<ICredentialProvider>(new ClientSecretCredentialProvider(clientSecret));
        }
    }
}