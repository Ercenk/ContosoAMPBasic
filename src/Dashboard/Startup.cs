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
using Dashboard.Controllers;
using Dashboard.Mail;
using Dashboard.Marketplace;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SaaSFulfillmentClient;

namespace Dashboard
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Subscriptions}/{action=Index}/{id?}");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                .AddAzureAD(options => this.Configuration.Bind("AzureAd", options));

            services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
            {
                options.Authority = options.Authority + "/v2.0/"; // Azure AD v2.0

                options.TokenValidationParameters.ValidateIssuer = false; // accept several tenants (here simplified)
            });

            services.Configure<DashboardOptions>(this.Configuration.GetSection("Dashboard"));

            services.AddFulfillmentClient(options => this.Configuration.Bind("FulfillmentClient", options),
                credentialBuilder =>
                    credentialBuilder.WithClientSecretAuthentication(this.Configuration["FulfillmentClient:AzureActiveDirectory:AppKey"]));

            services
                .AddWebhookProcessor()
                .WithWebhookHandler<ContosoWebhookHandler>();

            services.TryAddScoped<IFulfillmentManager, FulfillmentManager>();

            services.TryAddScoped<IEMailHelper, DashboardEMailHelper>();

            services.AddAuthorization(options =>
                options.AddPolicy("DashboardAdmin",
                    policy => policy.Requirements.Add((new DashboardAdminRequirement(this.Configuration
                        .GetSection("Dashboard").Get<DashboardOptions>().DashboardAdmin)))));

            services.AddSingleton<IAuthorizationHandler, DashboardAdminHandler>();

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
    }
}