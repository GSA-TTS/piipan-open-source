using System.Collections.Generic;

namespace Piipan.Shared.Deidentification
{
    public interface IRedactionService
    {
        /// <summary>
        /// Return a string with redactions
        /// </summary>
        /// <param name="errorDetails">The input string to check for redactions</param>
        /// <param name="pii">All of the values that need to be redacted</param>
        public string RedactParticipantsUploadError(string errorDetails, HashSet<string> pii);
    }
}