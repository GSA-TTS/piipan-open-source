using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Piipan.QueryTool.Tests.Extensions
{
    public static class ModelStateExtensions
    {
        public static void BindModel<T>(this PageModel pageModel, T model, string boundProperty, ValidationContext validationContext = null)
        {
            if (model == null) return;

            var context = validationContext ?? new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, context, results, true))
            {
                pageModel.ModelState.Clear();
                foreach (ValidationResult result in results)
                {
                    var key = (string.IsNullOrEmpty(boundProperty) ? "" : boundProperty + ".") +
                        result.MemberNames.FirstOrDefault() ?? "";
                    pageModel.ModelState.AddModelError(key, result.ErrorMessage);
                }
            }
        }
    }

}
