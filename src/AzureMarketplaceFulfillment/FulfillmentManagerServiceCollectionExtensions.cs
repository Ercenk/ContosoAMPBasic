using System;
using ContosoAssets.SolutionManagement.AzureMarketplaceFulfillment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaaSFulfillmentClient;

namespace AzureMarketplaceFulfillment
{
    public static class FulfillmentManagerServiceCollectionExtensions
    {
        public static void AddFulfillmentManager(this IServiceCollection services,
            Action<FulfillmentManagerOptions> configureOptions,
            Action<FulfillmentManagerBuilder> credentialBuilder)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services
                .AddOptions<FulfillmentManagerOptions>()
                .Configure(configureOptions);

            credentialBuilder(new FulfillmentManagerBuilder(services));
            services.TryAddScoped<IFulfillmentClient>(s =>
            {
                var logger = s.GetService<ILogger<FulfillmentClient>>();
                var options = s.GetService<IOptionsMonitor<FulfillmentManagerOptions>>();
                var fulfillmentServiceOptions = options.CurrentValue.FulfillmentService;

                return new FulfillmentClient(fulfillmentServiceOptions, logger);
            });

            services.TryAddScoped<IFulfillmentManager, FulfillmentManager>();
        }

        public static IServiceCollection AddWebhookProcessor(this IServiceCollection services)
        {
            services.TryAddScoped<IWebhookProcessor, WebhookProcessor>();
            return services;
        }

        public static void WithWebhookHandler<T>(this IServiceCollection services) where T : class, IWebhookHandler
        {
            services.TryAddScoped<IWebhookHandler, T>();
        }
    }
}