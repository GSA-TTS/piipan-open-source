using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Shared.Roles;
using Piipan.States.Api.Models;

namespace Piipan.Shared.Web
{
    /// <summary>
    /// This service provider gets all of the information needed while on the server to pass up to the client.
    /// The client will then use this data when displaying pages/components
    /// </summary>
    public interface IWebAppDataServiceProvider
    {
        /// <summary>
        /// This Initialize method should get called anytime this service provider is needed. It should fetch data from the server.
        /// </summary>
        /// <returns></returns>
        Task Initialize();

        string Email { get; }
        string HelpDeskEmail { get; set; }
        string Location { get; }
        string[] States { get; }
        bool IsNationalOffice { get; }
        string Role { get; }
        string BaseUrl { get; }
        RoleOptions AppRolesByArea { get; }
        StatesInfoResponse StateInfo { get; set; }
        StateInfoDto LoggedInUsersState { get; set; }
    }
}
