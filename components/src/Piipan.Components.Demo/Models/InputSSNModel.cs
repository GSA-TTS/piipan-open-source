using Piipan.Shared.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Components.Demo.Models
{
    /// <summary>
    /// A model that simulates an object that has a required ssn, and not required ssn.
    /// </summary>
    public class InputSSNModel
    {
        [UsaSSN]
        [Display(Name = "Optional SSN")]
        public string OptionalSSN { get; set; }

        [UsaSSN]
        [UsaRequired]
        [Display(Name = "Required SSN")]
        public string RequiredSSN { get; set; }
    }
}
