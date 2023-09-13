using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Piipan.Shared.API.Constants;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using Piipan.Shared.Roles;
using Piipan.Shared.Tests.Mocks;
using Piipan.Shared.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.Shared.Tests.Web
{
    /// <summary>
    /// Test the WebAppDataServiceProvider, validating the values that it provies. These values will be passed up to the client app.
    /// </summary>
    public class WebAppDataServiceProviderTests
    {
        /// <summary>
        /// Without calling WebAppDataServiceProvider.Initialize, certain values already have their values populated. Validate that they
        /// have values, and the others do not.
        /// </summary>
        [Fact]
        public void WebAppDataService_HasSomeValues_BeforeCallingInitialize()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            
            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(DefaultMocks.IClaimsProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(DefaultMocks.RoleProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(DefaultMocks.ILocationsProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(DefaultMocks.ConfigurationMock());

            HttpContextAccessor httpContextAccessor = new HttpContextAccessor() { HttpContext = DefaultMocks.HttpContextMock().Object };

            // Act
            WebAppDataServiceProvider webAppDataServiceProvider = new WebAppDataServiceProvider(serviceProviderMock.Object, httpContextAccessor);

            // Assert these values are null because Initialize hasn't been called yet.
            Assert.Null(webAppDataServiceProvider.StateInfo);
            Assert.Null(webAppDataServiceProvider.States);
            Assert.Null(webAppDataServiceProvider.LoggedInUsersState);
            Assert.False(webAppDataServiceProvider.IsNationalOffice);

            // Assert these values which are defined in the Mocks
            Assert.Equal("noreply@tts.test", webAppDataServiceProvider.Email);
            Assert.Equal("EA", webAppDataServiceProvider.Location);
            Assert.Equal("test@email.example", webAppDataServiceProvider.HelpDeskEmail);
            Assert.Equal("Worker", webAppDataServiceProvider.Role);
            Assert.Equal("https://tts.test", webAppDataServiceProvider.BaseUrl);
            Assert.Equal(new string[] { DefaultMocks.Role_Worker, DefaultMocks.Role_Oversight }, webAppDataServiceProvider.AppRolesByArea[RoleConstants.ViewMatchArea]);
            Assert.Equal(new string[] { DefaultMocks.Role_Worker }, webAppDataServiceProvider.AppRolesByArea[RoleConstants.EditMatchArea]);
        }

        /// <summary>
        /// After calling WebAppDataServiceProvider.Initialize, all values should be populated for this provider.
        /// Initialize is needed due to the asynchronous nature of how some of these properties are created.
        /// </summary>
        [Fact]
        public async Task WebAppDataService_HasAllValues_AfterCallingInitialize()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(DefaultMocks.IClaimsProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(DefaultMocks.RoleProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(DefaultMocks.ILocationsProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(DefaultMocks.ConfigurationMock());

            HttpContextAccessor httpContextAccessor = new HttpContextAccessor() { HttpContext = DefaultMocks.HttpContextMock().Object };

            // Act
            WebAppDataServiceProvider webAppDataServiceProvider = new WebAppDataServiceProvider(serviceProviderMock.Object, httpContextAccessor);
            await webAppDataServiceProvider.Initialize();

            // Assert these values are not null because Initialize has been called.
            Assert.Equal("EA", webAppDataServiceProvider.StateInfo.Results.ToArray()[0].StateAbbreviation);
            Assert.Equal("EB", webAppDataServiceProvider.StateInfo.Results.ToArray()[1].StateAbbreviation);
            Assert.Equal(new string[] { "EA" }, webAppDataServiceProvider.States);
            Assert.Equal("EA", webAppDataServiceProvider.LoggedInUsersState.StateAbbreviation);
            Assert.False(webAppDataServiceProvider.IsNationalOffice);

            // Assert these values which are defined in the Mocks
            Assert.Equal("noreply@tts.test", webAppDataServiceProvider.Email);
            Assert.Equal("EA", webAppDataServiceProvider.Location);
            Assert.Equal("test@email.example", webAppDataServiceProvider.HelpDeskEmail);
            Assert.Equal("Worker", webAppDataServiceProvider.Role);
            Assert.Equal("https://tts.test", webAppDataServiceProvider.BaseUrl);
            Assert.Equal(new string[] { DefaultMocks.Role_Worker, DefaultMocks.Role_Oversight }, webAppDataServiceProvider.AppRolesByArea[RoleConstants.ViewMatchArea]);
            Assert.Equal(new string[] { DefaultMocks.Role_Worker }, webAppDataServiceProvider.AppRolesByArea[RoleConstants.EditMatchArea]);
        }

        /// <summary>
        /// Validate that if the LocationsProvider returns back a list of states that has a wildcard, that we are treated as a national office user.
        /// </summary>
        [Fact]
        public async Task WebAppDataService_NationalOfficeValidation_AfterCallingInitialize()
        {
            // Arrange - Set location to National
            var serviceProviderMock = new Mock<IServiceProvider>();
            var claimsProviderMock = DefaultMocks.IClaimsProviderMock();
            claimsProviderMock
                .Setup(m => m.GetLocation(It.IsAny<ClaimsPrincipal>()))
                .Returns("National");

            serviceProviderMock.Setup(c => c.GetService(typeof(IClaimsProvider))).Returns(claimsProviderMock.Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IRolesProvider))).Returns(DefaultMocks.RoleProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(ILocationsProvider))).Returns(DefaultMocks.ILocationsProviderMock().Object);
            serviceProviderMock.Setup(c => c.GetService(typeof(IConfiguration))).Returns(DefaultMocks.ConfigurationMock());

            HttpContextAccessor httpContextAccessor = new HttpContextAccessor() { HttpContext = DefaultMocks.HttpContextMock().Object };

            // Act
            WebAppDataServiceProvider webAppDataServiceProvider = new WebAppDataServiceProvider(serviceProviderMock.Object, httpContextAccessor);
            await webAppDataServiceProvider.Initialize();

            // Assert these values are national office values.
            Assert.Equal("EA", webAppDataServiceProvider.StateInfo.Results.ToArray()[0].StateAbbreviation);
            Assert.Equal("EB", webAppDataServiceProvider.StateInfo.Results.ToArray()[1].StateAbbreviation);
            Assert.Equal(new string[] { "*" }, webAppDataServiceProvider.States);
            Assert.Null(webAppDataServiceProvider.LoggedInUsersState);
            Assert.True(webAppDataServiceProvider.IsNationalOffice);
        }
    }
}
