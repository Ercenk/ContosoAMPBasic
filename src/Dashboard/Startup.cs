namespace Dashboard
{
    using Dashboard.Mail;
    using Dashboard.Marketplace;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using SaaSFulfillmentClient;

    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseMvc(
                routes =>
                    {
                        routes.MapRoute(name: "default", template: "{controller=Subscriptions}/{action=Index}/{id?}");
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

            services.Configure<OpenIdConnectOptions>(
                AzureADDefaults.OpenIdScheme,
                options =>
                    {
                        options.Authority = options.Authority + "/v2.0/"; // Azure AD v2.0

                        options.TokenValidationParameters.ValidateIssuer =
                            false; // accept several tenants (here simplified)
                    });

            services.Configure<DashboardOptions>(this.configuration.GetSection("Dashboard"));

            services.AddFulfillmentClient(
                options => this.configuration.Bind("FulfillmentClient", options),
                credentialBuilder => credentialBuilder.WithClientSecretAuthentication(
                    this.configuration["FulfillmentClient:AzureActiveDirectory:AppKey"]));

            // Hack to save the host name and port during the handling the request. Please see the WebhookController and ContosoWebhookHandler implementations
            services.AddSingleton<ContosoWebhookHandlerOptions>();

            services.AddWebhookProcessor().WithWebhookHandler<ContosoWebhookHandler>();

            services.TryAddScoped<IFulfillmentManager, FulfillmentManager>();

            services.TryAddScoped<IEMailHelper, DashboardEMailHelper>();

            services.AddAuthorization(
                options => options.AddPolicy(
                    "DashboardAdmin",
                    policy => policy.Requirements.Add(
                        (new DashboardAdminRequirement(
                                this.configuration.GetSection("Dashboard").Get<DashboardOptions>().DashboardAdmin)))));

            services.AddSingleton<IAuthorizationHandler, DashboardAdminHandler>();

            services.AddMvc(
                options =>
                    {
                        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                        options.Filters.Add(new AuthorizeFilter(policy));
                    }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
    }
}