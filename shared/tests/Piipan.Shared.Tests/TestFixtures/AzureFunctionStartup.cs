using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Piipan.Shared.Tests.TestFixtures
{
    /// <summary>
    /// A barebones class that simply implements IFunctionsHostBuilder
    /// </summary>
    public class AzureFunctionEmptyBuilder : IFunctionsHostBuilder
    {
        private readonly IServiceCollection collection;

        public AzureFunctionEmptyBuilder(IServiceCollection collection)
        {
            this.collection = collection;
        }
        public IServiceCollection Services => collection;
    }

    /// <summary>
    /// A startup class that configures all routes to attempt to call functions registered instead of calling MVC controllers.
    /// This is used alongside PiipanAzureFunctionServer to spin up a mock server.
    /// </summary>
    public class AzureFunctionStartup
    {
        private readonly FunctionsStartup functionsStartup;
        private IServiceCollection serviceCollection;
        private Dictionary<Type, object> functionRegistrations = new Dictionary<Type, object>();

        /// <summary>
        /// Creates an AzureFunctionStartup from a service collection
        /// </summary>
        /// <param name="services"></param>
        public AzureFunctionStartup(ServiceCollection services)
        {
            serviceCollection = services;
        }

        /// <summary>
        /// Configures all of the services that were registered during IIntegrationTest.SetupServices
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            serviceCollection ??= services;
            serviceCollection.AddRouting();
            functionsStartup?.Configure(new AzureFunctionEmptyBuilder(serviceCollection));
        }

        /// <summary>
        /// Registers the Type alongside the corresponding AzureFunction that was created during startup
        /// </summary>
        /// <param name="type">The type of the API</param>
        /// <param name="azureFunction">The actual Azure Function API</param>
        public void Register(Type type, object azureFunction)
        {
            functionRegistrations.Add(type, azureFunction);
        }

        /// <summary>
        /// Configures the routing that transforms a HTTP Request to an actual call to the Azure Function.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.ApplicationServices = serviceCollection.BuildServiceProvider();
            var rb = new RouteBuilder(app);

            RequestDelegate factorialRequestHandler = async context =>
            {
                foreach (var type in functionRegistrations.Keys)
                {
                    if (!type.IsAbstract)
                    {
                        bool methodCalled = await TryCallAPI(type, context);
                        if (methodCalled)
                        {
                            return;
                        }
                    }
                }

                // If no methods were called, let the user know no API was found.
                await context.Response.WriteAsync("API Not found!");
            };
            rb.MapRoute("{*url}", factorialRequestHandler);

            var routes = rb.Build();

            app.UseRouter(routes);
        }

        /// <summary>
        /// Tries to match a route template to a request. This is used to know if we should be calling the Azure Function or not.
        /// </summary>
        /// <param name="routeTemplate">The route template to match to</param>
        /// <param name="requestPath">The actual request path we are calling</param>
        /// <param name="routeValues">The dictionary of all matched routes if any</param>
        /// <returns></returns>
        private bool TryMatch(string routeTemplate, string requestPath, out RouteValueDictionary routeValues)
        {
            routeValues = new RouteValueDictionary();
            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, GetDefaults(template));

            return matcher.TryMatch(requestPath, routeValues);
        }

        // This method extracts the default argument values from the template.
        private RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }

        private bool RouteMatches(ParameterInfo parameter, HttpContext context, out RouteValueDictionary routeValues)
        {
            var triggerAttr = parameter.GetCustomAttribute<HttpTriggerAttribute>();
            routeValues = null;
            return (triggerAttr.Route == null || TryMatch(triggerAttr.Route, context.Request.Path, out routeValues))
                && triggerAttr.Methods.Any(n => string.Equals(n, context.Request.Method, StringComparison.OrdinalIgnoreCase));
        }

        private async Task CallHttpRequest(HttpContext context, MethodInfo method, Type apiType, RouteValueDictionary routeValues)
        {
            List<object> methodParameters = new List<object>
            {
                context.Request // HttpRequest is the first parameter in every Azure Function
                                        };
            if (routeValues != null)
            {
                foreach (var val in routeValues)
                {
                    methodParameters.Add(val.Value);
                }
            }
            methodParameters.Add(NullLogger<AzureFunctionStartup>.Instance);
            var response = await (Task<IActionResult>)method.Invoke(functionRegistrations[apiType], methodParameters.ToArray());
            if (response is JsonResult jsonResult)
            {
                context.Response.StatusCode = jsonResult.StatusCode ?? 200;

                var serialized = JsonConvert.SerializeObject(jsonResult.Value);
                await context.Response.WriteAsync(serialized);
            }
        }

        private async Task<bool> TryCallAPI(Type type, HttpContext context)
        {
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute<FunctionNameAttribute>() != null)
                {
                    var parameters = method.GetParameters().Where(n => n.ParameterType == typeof(HttpRequest));
                    foreach (var para in parameters)
                    {
                        RouteValueDictionary routeValues = null;
                        if (RouteMatches(para, context, out routeValues))
                        {
                            await CallHttpRequest(context, method, type, routeValues);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
