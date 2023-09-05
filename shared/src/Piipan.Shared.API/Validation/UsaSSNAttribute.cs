using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Shared.API.Validation
{
    /// <summary>
    /// An attribute that defaults to the correct validation for social security numbers (SSN)
    /// </summary>
    public class UsaSSNAttribute : RegularExpressionAttribute
    {
        public UsaSSNAttribute()
            : base(@"^\d{3}-\d{2}-\d{4}$")
        {
            ErrorMessage = ValidationConstants.SSNInvalidFormatMessage;
        }

        public override bool IsValid(object value)
        {
            var stringValue = value?.ToString();
            // If the SSN is required, we'll pick it up with a UsaRequired attribute. Don't flag it here
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            List<string> allErrors = new List<string>();
            ErrorMessage = "";
            if (!base.IsValid(value))
            {
                allErrors.Add(ValidationConstants.SSNInvalidFormatMessage);
            }

            if (stringValue.Length >= 3 && int.TryParse(stringValue[0..3], out int areaNumber))
            {
                if (areaNumber == 0 || areaNumber == 666 || areaNumber >= 900)
                {
                    allErrors.Add(string.Format(ValidationConstants.SSNInvalidFirstThreeDigitsMessage, stringValue[0..3]));
                }
            }
            if (stringValue.Length >= 6 && stringValue[4..6] == "00")
            {
                allErrors.Add(ValidationConstants.SSNInvalidMiddleTwoDigitsMessage);
            }
            if (stringValue.Length >= 11 && stringValue[7..11] == "0000")
            {
                allErrors.Add(ValidationConstants.SSNInvalidLastFourDigitsMessage);
            }

            ErrorMessage = string.Join('\n', allErrors);

            return string.IsNullOrEmpty(ErrorMessage);
        }
    }
}
