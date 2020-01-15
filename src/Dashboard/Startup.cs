namespace Dashboard
{
    using Dashboard.Mail;
    using Dashboard.Marketplace;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.Routing.Patterns;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using SaaSFulfillmentClient;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Subscriptions}/{action=Index}/{id?}");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(
                options =>
                    {
                        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                        options.CheckConsentNeeded = context => true;
                        options.MinimumSameSitePolicy = SameSiteMode.None;
                    });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => this.configuration.Bind("AzureAd", options));

            services.Configure<CookieAuthenticationOptions>(
                AzureADDefaults.CookieScheme,
                options => options.AccessDeniedPath = "/Subscriptions/NotAuthorized");

            services.Configure<OpenIdConnectOptions>(
                AzureADDefaults.OpenIdScheme,
                options =>
                    {
                        options.Authority = options.Authority + "/v2.0/"; // Azure AD v2.0
                        options.TokenValidationParameters.ValidateIssuer =
                            false; // accept several tenants (here simplified)
                    });

            services.Configure<DashboardOptions>(this.configuration.GetSection("Dashboard"));

            services.AddFulfillmentClient(options => this.configuration.Bind("FulfillmentClient", options))
                .WithAzureTableOperationsStore(this.configuration["FulfillmentClient:OperationsStoreConnectionString"]);

            // Hack to save the host name and port during the handling the request. Please see the WebhookController and ContosoWebhookHandler implementations
            services.AddSingleton<ContosoWebhookHandlerOptions>();

            services.AddWebhookProcessor().WithWebhookHandler<ContosoWebhookHandler>();

            services.TryAddScoped<IFulfillmentHandler, FulfillmentHandler>();

            // It is email in this sample, but you can plug in anything that implements the interface and communicate with an existing API.
            // In the email case, the existing API is the SendGrid API...
            services.TryAddScoped<IMarketplaceNotificationHandler, DashboardEMailHelper>();

            services.AddAuthorization(
                options => options.AddPolicy(
                    "DashboardAdmin",
                    policy => policy.Requirements.Add(
                        new DashboardAdminRequirement(
                            this.configuration.GetSection("Dashboard").Get<DashboardOptions>().DashboardAdmin))));

            services.AddSingleton<IAuthorizationHandler, DashboardAdminHandler>();

            services.AddControllers(options =>
                {
                    var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }
            );
        }
    }
}