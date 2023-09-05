using Microsoft.Extensions.Configuration;
using Piipan.Shared.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.ConstrainedExecution;

namespace Piipan.Maintenance
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

            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                try
                {
                    context.Response.Headers.Add("X-Frame-Options", "DENY"); await next();

                    if (context.Response.StatusCode == 403 && (!context.Request.Path.Value?.TrimEnd('/').TrimEnd('\\').EndsWith("NotAuthorized", StringComparison.InvariantCultureIgnoreCase) ?? true)) { context.Response.Redirect("/Index"); }
                    if (context.Response.StatusCode == 500 && (!context.Request.Path.Value?.TrimEnd('/').TrimEnd('\\').EndsWith("ServerError", StringComparison.InvariantCultureIgnoreCase) ?? true)) { context.Response.Redirect("/Index"); }
                    if (context.Response.StatusCode == 404 || context.Response.StatusCode == 500) { context.Request.Path = "/Index"; context.Response.StatusCode = 200; await next(); }
                }
                catch { context.Request.Path = "/Index"; context.Response.StatusCode = 200; await next(); }
            });



            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<AuthenticationLoggingMiddleware>();

            app.MapRazorPages();

            app.Run();
        }
    }
}
