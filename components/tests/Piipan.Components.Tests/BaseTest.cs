using System;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Piipan.Components.Modals;
using Piipan.Components.Routing;

namespace Piipan.Components.Tests
{
    /// <summary>
    /// The Base Test for all components, which allows you to set initial values for the component, as well as
    /// perform operations such as creating and updating the component
    /// </summary>
    /// <typeparam name="T">The type of the component we are testing</typeparam>
    public abstract class BaseTest<T> : TestContext where T : IComponent, new()
    {
        public BaseTest()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;
            Services.AddModalManager();
            Services.AddPiipanNavigationManager();
        }
        protected T InitialValues { get; set; } = new T();

        protected IRenderedComponent<T>? Component { get; set; }

        protected void UpdateParameter<P>(Expression<Func<T, P>> parameter, P value)
        {
            Component?.SetParametersAndRender(parameters =>
                parameters.Add(parameter, value));
        }

        protected abstract void CreateTestComponent();
    }
}
