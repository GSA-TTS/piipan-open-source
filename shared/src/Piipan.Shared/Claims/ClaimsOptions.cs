namespace Piipan.Shared.Claims
{
    public class ClaimsOptions
    {
        public const string SectionName = "Claims";
        public string Email { get; set; }
        public string Role { get; set; }
        public string LocationPrefix { get; set; }
        public string RolePrefix { get; set; }
    }
}