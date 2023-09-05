using Piipan.States.Api.Models;

namespace Piipan.States.Core.Models
{
    /// <summary>
    /// Implementation of IState for database interactions
    /// </summary>
    public record StateInfoDbo : IState
    {
        public string Id { get; set; }
        public string State { get; set; }
        public string StateAbbreviation { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Region { get; set; }
        public string EmailCc { get; set; }
    }
}
