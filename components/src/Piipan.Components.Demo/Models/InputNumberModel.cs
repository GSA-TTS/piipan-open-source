using System.ComponentModel.DataAnnotations;
using Piipan.Shared.API.Validation;

namespace Piipan.Components.Demo.Models
{
    /// <summary>
    /// A model that simulates an object that has a required ssn, and not required ssn.
    /// </summary>
    public class InputNumberModel
    {
        [Display(Name = "Optional Number")]
        public int OptionalNumber { get; set; }

        [UsaRequired]
        [Display(Name = "Required Number")]
        public int RequiredNumber { get; set; }
    }
}
