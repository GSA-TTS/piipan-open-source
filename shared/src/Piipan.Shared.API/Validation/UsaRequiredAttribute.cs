using System.ComponentModel.DataAnnotations;

namespace Piipan.Shared.API.Validation
{
    /// <summary>
    /// A required attribute that defaults to the correct validation error message
    /// </summary>
    public class UsaRequiredAttribute : RequiredAttribute
    {
        public UsaRequiredAttribute() : base()
        {
            ErrorMessage = ValidationConstants.RequiredMessage;
        }
    }
}
