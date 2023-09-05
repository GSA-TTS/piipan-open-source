using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Piipan.Shared.Claims;
using Piipan.Shared.Locations;
using Piipan.Shared.Roles;
using Piipan.States.Api.Models;

namespace Piipan.Shared.Web
{
    /// <summary>
    /// This service provider gets all of the information needed while on the server to pass up to the client.
    /// The client will then use this data when displaying pages/components
    /// </summary>
    public class WebAppDataServiceProvider : IWebAppDataServiceProvider
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IRolesProvider _rolesProvider;
        private readonly ILocationsProvider _locationsProvider;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// Service provider constructor that gets the main application IServiceProvider and also the IHttpContextAccessor injected into it.
        /// Subsequent providers are retrieved from the IServiceProvider
        /// </summary>
        public WebAppDataServiceProvider(IServiceProvider serviceProvider, IHttpContextAccessor contextAccessor)
        {
            _claimsProvider = serviceProvider.GetRequiredService<IClaimsProvider>();
            _rolesProvider = serviceProvider.GetRequiredService<IRolesProvider>();
            _locationsProvider = serviceProvider.GetRequiredService<ILocationsProvider>();
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _contextAccessor = contextAccessor;

            HelpDeskEmail = _configuration["HelpDeskEmail"];
        }

        /// <summary>
        /// This Initialize method should get called anytime this service provider is needed. It should fetch data from the server.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            StateInfo = await _locationsProvider.GetStatesFromStatesApi();
            States = await _locationsProvider.GetStates(Location);
            LoggedInUsersState = StateInfo?.Results?.FirstOrDefault(n => n.StateAbbreviation.Equals(Location, StringComparison.OrdinalIgnoreCase));
        }

        public string Email
        {
            get { return _claimsProvider.GetEmail(_contextAccessor.HttpContext.User); }
        }

        public string Location
        {
            get { return _claimsProvider.GetLocation(_contextAccessor.HttpContext.User); }
        }

        public string HelpDeskEmail { get; set; }

        public string[] States { get; set; }
        public bool IsNationalOffice => States?.Contains("*") ?? false;

        public string Role
        {
            get { return _claimsProvider.GetRole(_contextAccessor.HttpContext.User); }
        }
        public string BaseUrl
        {
            get { return $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}"; }
        }

        public RoleOptions AppRolesByArea => _rolesProvider.GetRoles();

        public StatesInfoResponse StateInfo { get; set; }
        public StateInfoDto LoggedInUsersState { get; set; }
    }
}
