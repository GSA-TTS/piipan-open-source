using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Piipan.Shared.API.Validation
{
    public class UsaNameAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var stringValue = value?.ToString()?.Trim();

            // If the name is required, we'll pick it up with a UsaRequired attribute. Don't flag it here
            if (string.IsNullOrEmpty(stringValue))
            {
                return true;
            }

            // Loud failure for non-ascii chars
            string nonasciirgx = @"[^\x00-\x7F]";
            MatchCollection matches = Regex.Matches(stringValue, nonasciirgx, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                List<string> invalidValues = new List<string>();
                foreach (Match match in matches)
                {
                    if (!invalidValues.Contains(match.Value))
                    {
                        invalidValues.Add(match.Value);
                    }
                }
                ErrorMessage = string.Format(ValidationConstants.InvalidCharacterInNameMessage, string.Join(',', invalidValues), stringValue);
                return false;
            }

            if (!char.IsLetter(stringValue[0])) // not at least one char
            {
                ErrorMessage = ValidationConstants.MustStartWithALetter;
                return false;
            }
            return true;
        }
    }
}
