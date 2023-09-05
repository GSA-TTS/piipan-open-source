using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Represents a service to exclude PII from a string
    /// </summary>
    public class RedactionService : IRedactionService
    {
        /// <summary>
        /// Return a string with redactions
        /// </summary>
        /// <param name="errorDetails">The input string to check for redactions</param>
        /// <param name="pii">All of the values that need to be redacted</param>
        public string RedactParticipantsUploadError(string errorDetails, HashSet<string> pii)
        {
            //remove all characters except [a-zA-Z0-9_-/+=]
            var alphanumericRegexWithEmptySpace = @"[^\w\s\-\/\+\=]";
            var validErrorMessagesArray = Regex.Replace(errorDetails, alphanumericRegexWithEmptySpace, " ")
                .Replace("\r\n", " ").Split(" ").Where(c => !string.IsNullOrEmpty(c)).ToArray();

            foreach (var errorWord in validErrorMessagesArray)
            {
                if (pii.Contains(errorWord))
                    errorDetails = errorDetails.Replace(errorWord, "REDACTED", StringComparison.InvariantCultureIgnoreCase);
            }

            return errorDetails;
        }
    }
}