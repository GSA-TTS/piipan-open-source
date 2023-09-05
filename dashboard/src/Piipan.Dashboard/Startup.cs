using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NEasyAuthMiddleware;
using Piipan.Metrics.Client.Extensions;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Client.DTO;
using Piipan.Shared.Locations;
using Piipan.Shared.Logging;
using Piipan.Shared.Roles;
using Piipan.Shared.Web;
using Piipan.States.Client.Extensions;

namespace Piipan.Dashboard
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ClaimsOptions>(Configuration.GetSection(ClaimsOptions.SectionName));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            });

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            }).AddApplicationPart(typeof(Controllers.UploadsController).Assembly); // One of the controllers must be added to get the DLL Loaded for integration tests

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/SignedOut");
            });

            services.AddHsts(options =>
            {
                options.Preload = true; //Sets the preload parameter of the Strict-Transport-Security header. Preload isn't part of the RFC HSTS specification, but is supported by web browsers to preload HSTS sites on fresh install. For more information, see https://hstspreload.org
                options.IncludeSubDomains = true; //Enables includeSubDomain, which applies the HSTS policy to Host subdomains.
                options.MaxAge = TimeSpan.FromSeconds(31536000); //Explicitly sets the max-age parameter of the Strict-Transport-Security header to 365 days. If not set, defaults to 30 days.
            });

            services.AddHttpContextAccessor();
            services.AddEasyAuth();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddAuthorizationCore(options =>
            {
                options.DefaultPolicy = AuthorizationPolicyBuilder.Build(Configuration
                    .GetSection(AuthorizationPolicyOptions.SectionName)
                    .Get<AuthorizationPolicyOptions>());
            });

            services.AddTransient<IClaimsProvider, ClaimsProvider>();
            services.AddTransient<ILocationsProvider, LocationsProvider>();
            services.AddTransient<IRolesProvider, RolesProvider>();

            services.RegisterMetricsClientServices(_env);
            services.AddTransient<ClientAppDataDto>();
            services.RegisterStatesClientServices(_env);
            services.AddTransient<IWebAppDataServiceProvider, WebAppDataServiceProvider>();

            if (_env.IsDevelopment())
            {
                var mockFile = $"{_env.ContentRootPath}/mock_user.json";
                services.UseJsonFileToMockEasyAuth(mockFile);
            }

            services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddApplicationInsightsTelemetry((options) =>
            {
                options.ConnectionString = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                await next();

                if (!context.Request.Path.Value.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                {
                    if (context.Response.StatusCode == 403 &&
                        (!context.Request.Path.Value?.TrimEnd('/').TrimEnd('\\').EndsWith("NotAuthorized", StringComparison.InvariantCultureIgnoreCase) ?? true))
                    {
                        context.Response.Redirect("/NotAuthorized");
                    }
                    if (context.Response.StatusCode == 500 &&
                        (!context.Request.Path.Value?.TrimEnd('/').TrimEnd('\\').EndsWith("ServerError", StringComparison.InvariantCultureIgnoreCase) ?? true))
                    {
                        context.Response.Redirect("/ServerError");
                    }
                }
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<AuthenticationLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers().RequireAuthorization();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
