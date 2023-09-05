using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Shared.API.Validation
{
    /// <summary>
    /// UsaRequiredIf can be used to make one property dependent on others. 
    /// Unfortunately, the CLR can only take in an array of strings, and cannot accept classes or tuples
    /// See https://github.com/dotnet/roslyn/issues/22459 for more info.
    /// Therefore, we accept an array of string parameters that should be defined in groups of 3.
    /// First is the source property name, second is the source property test value, and third is the error message.
    /// If you have another property to test, you can add it as a 4th, 5th, and 6th string parameter.
    /// For example:
    /// 
    /// public class Foo
    /// {
    ///    public string Prop1 { get; set; }
    ///    
    ///    [UsaRequiredIf(nameof(Prop1), "", "Error Message")]
    ///    public string Prop2 { get; set; } // This property is required ONLY if Prop1 has a value and will show the error message "Error Message"
    ///    
    ///    [UsaRequiredIf(
    ///       nameof(Prop1), "Test", "Prop3 is required",
    ///       nameof(Prop2), "", "Prop3 is required"
    ///    )]
    ///    public string Prop3 { get; set; } // This property is required ONLY if Prop1 has the value of "Test" or if Prop2 has any value
    /// }
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class UsaRequiredIfAttribute : ValidationAttribute
    {
        public (string sourceProperty, string sourcePropertyTestValue, string errorMessage)[] Dependencies { get; set; } = Array.Empty<(string sourceProperty, string sourcePropertyTestValue, string errorMessage)>();

        public UsaRequiredIfAttribute(params string[] dependencyInfo) : base()
        {
            ErrorMessage = ValidationConstants.RequiredMessage;
            List<(string sourceProperty, string sourcePropertyTestValue, string errorMessage)> list = new();

            // Populate the dependencies array.
            for (int i = 0; i + 2 < dependencyInfo.Length; i += 3)
            {
                list.Add((dependencyInfo[i], dependencyInfo[i + 1], dependencyInfo[i + 2]));
            }
            Dependencies = list.ToArray();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string currentValueAsString = value?.ToString();

            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();

            foreach (var dependency in Dependencies)
            {
                var sourcePropertyValue = type.GetProperty(dependency.sourceProperty).GetValue(instance, null);

                // If the source property test value doesn't have a value, we should make our property required if the source property has ANY value
                if (string.IsNullOrEmpty(dependency.sourcePropertyTestValue))
                {
                    if (!string.IsNullOrEmpty(sourcePropertyValue?.ToString()) && string.IsNullOrEmpty(currentValueAsString))
                    {
                        return new ValidationResult(dependency.errorMessage, new string[] { validationContext.MemberName });
                    }
                }
                // If the source property test value DOES have a value, we should make our property required ONLY if the source property value is equal to the test value
                else
                {
                    if (sourcePropertyValue?.ToString() == dependency.sourcePropertyTestValue && string.IsNullOrEmpty(currentValueAsString))
                    {
                        return new ValidationResult(dependency.errorMessage, new string[] { validationContext.MemberName });
                    }
                }
            }


            return ValidationResult.Success;
        }
    }
}
