using System;
using System.ComponentModel.DataAnnotations;
using Piipan.Shared.API.Validation;

namespace Piipan.Components.Demo.Models
{
    /// <summary>
    /// A model that simulates an object that has a required date, and not required date.
    /// </summary>
    public class InputDateModel
    {
        [Display(Name = "Optional Date")]
        public DateTime? NotRequiredDate { get; set; }

        [UsaRequired]
        [Display(Name = "Required Date")]
        public DateTime? RequiredDate { get; set; }
    }
}
