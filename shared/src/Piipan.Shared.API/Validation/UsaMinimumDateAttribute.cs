using System;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Shared.API.Validation
{
    /// <summary>
    /// UsaMinimumDate can be used to make one property dependent on another one's date. For example:
    /// 
    /// public class Foo
    /// {
    ///    public DateTime? Prop1 { get; set; }
    ///    
    ///    [UsaMinimumDate(nameof(Prop1))]
    ///    public DateTime? Prop2 { get; set; } // This property must be greater than or equal to Prop1 if both Properties have a value
    /// }
    /// </summary>
    public class UsaMinimumDateAttribute : ValidationAttribute
    {
        public string SourceProperty { get; set; }

        public UsaMinimumDateAttribute(string sourceProperty) : base()
        {
            SourceProperty = sourceProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();


            var sourcePropertyValue = type.GetProperty(SourceProperty).GetValue(instance, null) as DateTime?;
            var testValueAsDate = value as DateTime?;

            // If the source property test value doesn't have a value, we should make our property required if the source property has ANY value
            if (sourcePropertyValue != null && testValueAsDate != null && testValueAsDate.Value.Date < sourcePropertyValue.Value.Date)
            {
                ErrorMessage = ErrorMessage.Replace("{0}", sourcePropertyValue.Value.ToString("MM/dd/yyyy"));
                return new ValidationResult(ErrorMessage, new string[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
