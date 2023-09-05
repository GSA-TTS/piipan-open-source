using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piipan.Shared.Tests.Exceptions;
using Xunit;

namespace Piipan.Shared.Tests.DependencyInjection
{
    /// <summary>
    /// A tester class that verifies all dependencies can be injected into every registered type.
    /// Call .Register for every type you want to verify, and then call ValidateFunctionServices to validate all registered types
    /// can be instantiated with the dependency injector.
    /// </summary>
    public class DependencyTester
    {
        private List<Type> dependencyList = new();

        /// <summary>
        /// Registers an Entry Point class. This can be something like an Azure Function class, Controller, or Page.
        /// </summary>
        /// <typeparam name="TEntryPoint">An entry point class of an application, such as an Azure Function or a controller</typeparam>
        /// <returns>Returns itself with the entry point to be verified</returns>
        public DependencyTester Register<TEntryPoint>()
        {
            dependencyList.Add(typeof(TEntryPoint));
            return this;
        }

        public DependencyTester Register(Type type)
        {
            dependencyList.Add(type);
            return this;
        }

        /// <summary>
        /// Validates Azure Functions, verifying that they can be instantiated and that all required dependencies
        /// have been registered and can be injected.
        /// Also validates to make sure all entry points are registered and accounted for by looking 
        /// for any class that has any method with a FunctionName attribute that is in the same assembly as the Startup.
        /// </summary>
        /// <typeparam name="TStartup">The type of the Azure Functions Startup file</typeparam>
        public void ValidateFunctionServices<TStartup>() where TStartup : FunctionsStartup, new()
        {
            var assembly = typeof(TStartup).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract)
                {
                    foreach (var method in type.GetMethods())
                    {
                        if (method.GetCustomAttribute<FunctionNameAttribute>() != null)
                        {
                            Assert.Contains(type, dependencyList);
                        }
                    }
                }
            }

            FunctionsStartup startup = new TStartup();
            var app = new HostBuilder().ConfigureWebJobs((builder) =>
            {
                startup.Configure(builder);
                PerformValidation(builder.Services);
            });

            // Look for a TestSuccessException exception.
            // This mimicks Assert.Pass from NUnit
            Assert.Throws<TestSuccessException>(app.Build);
        }

        /// <summary>
        /// Validates Web Apps, verifying that they can be instantiated and that all required dependencies
        /// have been registered and can be injected.
        /// Also validates to make sure all entry points are registered and accounted for by looking 
        /// for any class that inherits from either PageModel or ControllerBase that is in the same assembly as the Startup.
        /// </summary>
        /// <typeparam name="TStartup">The type of the Web App Startup file</typeparam>
        /// <param name="hostBuilderFunc">The function that builds the IHostBuilder. Normally this is Program.CreateHostBuilder</param>
        public void ValidateWebServices<TStartup>(Func<string[], IHostBuilder> hostBuilderFunc) where TStartup : class
        {
            var assembly = typeof(TStartup).Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType == typeof(ControllerBase) || type.BaseType?.BaseType == typeof(ControllerBase)
                    || type.BaseType == typeof(PageModel) || type.BaseType?.BaseType == typeof(PageModel))
                {
                    if (!type.IsAbstract)
                    {
                        Assert.Contains(type, dependencyList);
                    }
                }
            }

            var hostBuilder = hostBuilderFunc(null);
            hostBuilder.ConfigureServices(services =>
            {
                PerformValidation(services);
            });
            Assert.Throws<TestSuccessException>(hostBuilder.Build);
        }

        /// <summary>
        /// Goes through all dependencies and make sure they can be instantiated using the given IServiceCollection
        /// </summary>
        /// <param name="serviceCollection">The collection of registered services</param>
        private void PerformValidation(IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();

            foreach (var dependency in dependencyList)
            {
                foreach (var constructor in dependency.GetConstructors())
                {
                    var parameters = constructor.GetParameters();
                    constructor.Invoke(parameters.Select(n => serviceProvider.GetRequiredService(n.ParameterType)).ToArray());
                }
            }

            // Throw an exception to exit early, so as to not complete the rest of ConfigureWebJobs.
            // This mimicks Assert.Pass from NUnit
            throw new TestSuccessException();
        }
    }
}