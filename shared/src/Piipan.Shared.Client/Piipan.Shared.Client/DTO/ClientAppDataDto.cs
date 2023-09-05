using Piipan.States.Api.Models;

namespace Piipan.Shared.Client.DTO
{
    public class ClientAppDataDto
    {
        public string Email { get; set; } = "";

        public string Location { get; set; } = "";

        public string[] States { get; set; } = Array.Empty<string>();

        public bool IsNationalOffice => States?.Contains("*") ?? false;

        public string Role { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public Dictionary<string, string[]> AppRolesByArea { get; set; } = new();

        public StatesInfoResponse? StateInfo { get; set; }

        public StateInfoDto? LoggedInUsersState { get; set; }

        private string currentPage = "";
        public string CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                if (CurrentPageUpdated != null)
                {
                    CurrentPageUpdated.Invoke();
                }
            }
        }
        public Func<Task>? CurrentPageUpdated { get; set; }

        public string HelpDeskEmail { get; set; }
    }
}
