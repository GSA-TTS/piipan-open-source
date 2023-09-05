using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Piipan.Etl.Func.BulkUpload.Validators
{
    /// <summary>
    /// Validates a date time or date range values from a CSV file formatted in accordance with
    /// <c>/etl/docs/csv/import-schema.json</c>.
    /// </summary>
    public class DateValidateHelper
    {
        /// <summary>
        /// Validates date time in accordance with
        /// <c>/etl/docs/csv/import-schema.json</c>. 
        /// </summary>
        public bool IsValidParticipantClosingDate(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            string[] formats = { "yyyy-MM-dd" };
            DateTime dateValue;
            var result = DateTime.TryParseExact(
                value, formats, new CultureInfo("en-US"),
                DateTimeStyles.None, out dateValue);

            if (!result)
                return false;

            return true;
        }

        /// <summary>
        /// Validates date range in accordance with
        /// <c>/etl/docs/csv/import-schema.json</c>. 
        /// </summary>
        public bool IsValidRecentBenefitIssuanceDates(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            string[] formats = { "yyyy-MM-dd", "yyyy-M-dd" };
            string[] dateRanges = value.Split(' ');

            if (dateRanges.Length > 3)
                return false;

            foreach (string dateRange in dateRanges)
            {
                string[] dates = dateRange.Split('/');

                if (dates.Length < 2)
                    return false;

                DateTime startValue;

                var result = DateTime.TryParseExact(
                    dates[0], formats, new CultureInfo("en-US"),
                    DateTimeStyles.None, out startValue);

                if (!result)
                    return false;

                DateTime endValue;
                result = DateTime.TryParseExact(
                    dates[1], formats, new CultureInfo("en-US"),
                    DateTimeStyles.None, out endValue);

                if (!result)
                    return false;

                if (endValue < startValue)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validate field by alphanumeric and length
        /// </summary>
        public bool ValidateAlphaNumericWithLength(string field)
        {
            if (field == null)
                return false;

            var match = Regex.Match(field, "^[A-Za-z0-9-_]+$");

            if (!match.Success)
                return false;

            if (field.Length > 20)
                return false;

            return true;
        }

        /// <summary>
        /// Validate lds_hash by regular expression
        /// </summary>
        public bool ValidateLdsHash(string field)
        {
            if (field == null)
                return false;

            var match = Regex.Match(field, "^[0-9a-f]{128}$");
            return match.Success;
        }

        /// <summary>
        /// Validate caseId by null or empty and regular expression
        /// </summary>
        public bool ValidateCaseId(string field)
        {
            if (string.IsNullOrEmpty(field))
                return true;

            return ValidateAlphaNumericWithLength(field);
        }
    }
}
