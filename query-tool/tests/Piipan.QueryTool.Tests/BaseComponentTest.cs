using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Piipan.Components.Modals;
using Piipan.Components.Routing;
using Piipan.QueryTool.Client.Api;
using Piipan.QueryTool.Client.Helpers;
using Piipan.Shared.API.Constants;
using Piipan.Shared.Client.DTO;
using Piipan.States.Api.Models;

namespace Piipan.QueryTool.Tests
{
    /// <summary>
    /// The Base Test for all Blazor components, which allows you to set initial values for the component, as well as
    /// perform operations such as creating and updating the component
    /// </summary>
    /// <typeparam name="T">The type of the component we are testing</typeparam>
    public abstract class BaseComponentTest<T> : TestContext where T : IComponent, new()
    {
        protected T InitialValues { get; set; } = new T();
        protected ClientAppDataDto AppData { get; set; }
            = new ClientAppDataDto()
            {
                Email = "test@usda.example",
                HelpDeskEmail = "help@usda.example",
                Location = "EA",
                States = new string[] { "EA" },
                Role = "Worker",
                BaseUrl = "https://test.usda.example",
                AppRolesByArea = new Dictionary<string, string[]>() {
                        { RoleConstants.ViewMatchArea, new string[] { "Worker", "Oversight" }},
                        { RoleConstants.EditMatchArea, new string[] { "Worker" }}
                    },
                StateInfo = new States.Api.Models.StatesInfoResponse
                {
                    Results = new List<StateInfoDto> {
                            new StateInfoDto{
                                Email = "ea@usda.example",
                                State = "Echo Alpha",
                                StateAbbreviation = "EA"
                            },
                            new StateInfoDto{
                                Email = "eb@usda.example",
                                State = "Echo Bravo",
                                StateAbbreviation = "EB"
                            }
                        }
                },
                LoggedInUsersState = new StateInfoDto
                {
                    Email = "ea@usda.example",
                    State = "EA"
                }
            };

        protected IRenderedComponent<T> Component { get; set; }
        protected Mock<IQueryToolApiService> MockApiService { get; set; } = new Mock<IQueryToolApiService>();

        protected void UpdateParameter<P>(Expression<Func<T, P>> parameter, P value)
        {
            Component?.SetParametersAndRender(parameters =>
                parameters.Add(parameter, value));
        }

        public BaseComponentTest()
        {
            JSInterop.Mode = JSRuntimeMode.Loose;

            Services.AddTransient((serviceProvider) => MockApiService.Object);
            Services.AddModalManager();
            Services.AddPiipanNavigationManager();
            Services.TryAddSingleton<ClientAppDataDto>();
        }
        protected virtual void CreateTestComponent()
        {
            var appData = Services.GetService<ClientAppDataDto>();
            PropertyCopier.CopyPropertiesTo(AppData, appData);
        }
    }
}
