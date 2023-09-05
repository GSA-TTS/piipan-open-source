using System;
using Microsoft.Extensions.DependencyInjection;

namespace Piipan.Shared.Tests.TestFixtures
{
    /// <summary>
    /// Interface which should be implemented by all Azure Function integration tests. These will allow for Cypress integration tests
    /// and QT/Dashboard Integration tests.
    /// </summary>
    public interface IIntegrationTest
    {
        /// <summary>
        /// Returns the Azure Function API of the given type
        /// </summary>
        /// <param name="type">The type of the Azure Function to find</param>
        /// <returns>An Azure Function object reference</returns>
        public object GetApi(Type type);

        /// <summary>
        /// Set up the environment variables and other services that the tests will use
        /// </summary>
        /// <returns>The service collection with environment variables and services registered</returns>
        public ServiceCollection SetupServices();
    }
}
