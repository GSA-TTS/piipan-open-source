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
using Piipan.Match.Client.Extensions;
using Piipan.QueryTool.Binders;
using Piipan.QueryTool.Controllers;
using Piipan.Shared.Authorization;
using Piipan.Shared.Claims;
using Piipan.Shared.Client.DTO;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Locations;
using Piipan.Shared.Logging;
using Piipan.Shared.Roles;
using Piipan.Shared.Web;
using Piipan.States.Client.Extensions;

namespace Piipan.QueryTool
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
        protected readonly IWebHostEnvironment _env;

        protected virtual void UseMockFile(IServiceCollection services)
        {
            services.UseJsonFileToMockEasyAuth($"{_env.ContentRootPath}/mock_user.json");
        }

        protected virtual void RegisterMatchClientServices(IServiceCollection services)
        {
            services.RegisterMatchClientServices(_env);
        }

        protected virtual void RegisterMatchResolutionClientServices(IServiceCollection services)
        {
            services.RegisterMatchResolutionClientServices(_env);
        }

        protected virtual void RegisterStatesClientServices(IServiceCollection services)
        {
            services.RegisterStatesClientServices(_env);
        }

        protected virtual void FinishConfiguringServices(IApplicationBuilder applicationBuilder)
        {
            // Nothing to do here. Cypress test project will override this.
        }

        protected virtual void AddStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<LocationOptions>(Configuration.GetSection(LocationOptions.SectionName));
            services.Configure<RoleOptions>(Configuration.GetSection(RoleOptions.SectionName));
            services.Configure<ClaimsOptions>(Configuration.GetSection(ClaimsOptions.SectionName));

            services.AddServerSideBlazor();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            });

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            }).AddApplicationPart(typeof(DuplicateParticipantSearchController).Assembly); // One of the controllers must be added to get the DLL Loaded for integration tests

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/SignedOut");
            }).AddMvcOptions(options =>
            {
                options.ModelBinderProviders.Insert(0, new TrimModelBinderProvider());
            }).AddApplicationPart(typeof(DuplicateParticipantSearchController).Assembly);

            services.AddHttpClient();

            services.AddHsts(options =>
            {
                options.Preload = true; //Sets the preload parameter of the Strict-Transport-Security header. Preload isn't part of the RFC HSTS specification, but is supported by web browsers to preload HSTS sites on fresh install. For more information, see https://hstspreload.org
                options.IncludeSubDomains = true; //Enables includeSubDomain, which applies the HSTS policy to Host subdomains.
                options.MaxAge = TimeSpan.FromSeconds(31536000); //Explicitly sets the max-age parameter of the Strict-Transport-Security header to 365 days. If not set, defaults to 30 days.
            });

            services.AddTransient<IClaimsProvider, ClaimsProvider>();
            services.AddTransient<ILocationsProvider, LocationsProvider>();
            services.AddTransient<IRolesProvider, RolesProvider>();

            services.AddSingleton<INameNormalizer, NameNormalizer>();
            services.AddSingleton<IDobNormalizer, DobNormalizer>();
            services.AddSingleton<ISsnNormalizer, SsnNormalizer>();
            services.AddSingleton<ILdsHasher, LdsHasher>();
            services.AddSingleton<ILdsDeidentifier, LdsDeidentifier>();

            services.AddTransient<ClientAppDataDto>();

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

            RegisterMatchClientServices(services);
            RegisterMatchResolutionClientServices(services);
            RegisterStatesClientServices(services);
            services.AddTransient<IWebAppDataServiceProvider, WebAppDataServiceProvider>();

            if (_env.IsDevelopment())
            {
                UseMockFile(services);
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
            app.UseStatusCodePagesWithReExecute("/NotAuthorized", "?code={0}");

            //Perform middleware for custom 404 page
            app.Use(async (context, next) =>
            {
                try
                {
                    context.Response.Headers.Add("X-Frame-Options", "DENY");
                    await next();

                    // Ignore for API calls. This should only be for page loads
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
                        if (context.Response.StatusCode == 404 || context.Response.StatusCode == 500)
                        {
                            context.Request.Path = "/Error";
                            context.Response.StatusCode = 200;
                            await next();
                        }
                    }
                }
                catch
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Request.Path = "/Error";
                        context.Response.StatusCode = 200;
                        await next();
                    }
                }
            });
            AddStaticFiles(app, env);

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

            FinishConfiguringServices(app);
        }
    }
}
