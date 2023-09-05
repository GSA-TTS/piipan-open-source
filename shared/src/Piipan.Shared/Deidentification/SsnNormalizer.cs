using System;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    public class SsnNormalizer : ISsnNormalizer
    {
        /// <summary>
        /// Public entrypoint for functionality.
        /// Normalizes social security number according to PPRL specifications.
        /// </summary>
        /// <param name="ssn">social security number of individual in the format of a 3-digit area number, a 2-digit group number, and a 4-digit serial number, in this order, all separated by a hyphen</param>
        public string Run(string ssn)
        {
            Regex ssnRgx = new Regex(@"^\d{3}-\d{2}-\d{4}$");
            if (!ssnRgx.IsMatch(ssn))
            {
                throw new ArgumentException("Social security number must have a 3-digit area number, a 2-digit group number, and a 4-digit serial number, in this order, all separated by a hyphen.");
            }
            string[] numberGroups = ssn.Split("-");
            // Area numbers 000, 666, and 900-999 are invalid
            string area = numberGroups[0];
            if (String.Equals(area,"000") ||
                String.Equals(area,"666") ||
                area.StartsWith("9"))
            {
                throw new ArgumentException("The first three numbers of SSN cannot be 000, 666, or between 900-999.");
            }
            // Group number 00 is invalid
            string group = numberGroups[1];
            if (String.Equals(group, "00"))
            {
                throw new ArgumentException("The middle two numbers of SSN cannot be 00.");
            }
            // Serial number 0000 is invalid
            string serial = numberGroups[2];
            if (String.Equals(serial, "0000"))
            {
                throw new ArgumentException("The last four numbers of SSN cannot be 0000.");
            }
            return ssn;
        }
    }
}
