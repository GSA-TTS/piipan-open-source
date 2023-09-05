using System.ComponentModel.DataAnnotations;
using Piipan.Shared.API.Validation;

namespace Piipan.Components.Demo.Models
{
    /// <summary>
    /// A model that simulates an object that has a required text field, and not required text field.
    /// </summary>
    public class InputTextModel
    {
        [Display(Name = "Optional Field")]
        public string NotRequiredField { get; set; }

        [UsaRequired]
        [Display(Name = "Required Field")]
        public string RequiredField { get; set; }

        [UsaRequiredIf(
            nameof(RequiredField), null, "@@@ is required when RequiredField has a value",
            nameof(NotRequiredField), null, "@@@ is required when NotRequiredField has a value"
        )]
        [Display(Name = "Required If Field")]
        public string RequiredIfField { get; set; }
    }
}
