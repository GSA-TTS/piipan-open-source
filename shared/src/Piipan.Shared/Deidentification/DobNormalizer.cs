using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Normalizes date of birth according to PPRL specifications.
    /// </summary>
    public class DobNormalizer : IDobNormalizer
    {
        /// <summary>
        /// Public entrypoint for class.
        /// </summary>
        /// <param name="dob">date of birth of individual in string format yyyy-MM-dd</param>

        public const int MaxYearsAgo = 130;
        public string Run(string dob)
        {
            try
            {
                // InvariantCulture.Calendar is GregorianCalendar
                DateTime.ParseExact(dob, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Dates must be in ISO 8601 format using a 4-digit year, a zero-padded month, and zero-padded day. Date must exist on Gregorian calendar.");
            }
            if (DateTime.Parse(dob).CompareTo(DateTime.Now.AddYears(-MaxYearsAgo)) < 0)
            {
                throw new ArgumentException($"Date should be later than {MaxYearsAgo} years ago.");
            }
            return dob;
        }
    }
}
