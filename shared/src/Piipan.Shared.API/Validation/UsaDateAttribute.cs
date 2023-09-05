using System;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Shared.API.Validation
{
    public class UsaDateAttribute : RangeAttribute
    {
        public const string Today = "Today";
        public const string EarliestDefaultDate = "01/01/1900";
        public const string LatestDefaultDate = "12/31/2050";

        /// <summary>
        /// Puts Date validation on the given date. If you want to validate either the minimum or maximum date compared to today,
        /// pass in the word "today"
        /// </summary>
        /// <param name="MinValue">The string version of the date, or "Today". You can also pass in null to make it so there is no min value.</param>
        /// <param name="MaxValue">The string version of the date, or "Today". You can also pass in null to make it so there is no max value.</param>
        public UsaDateAttribute(string? MinValue = EarliestDefaultDate, string? MaxValue = LatestDefaultDate)
            : base(typeof(DateTime),
                  MinValue == Today ? DateTime.Now.ToShortDateString() : MinValue ?? DateTime.MinValue.ToShortDateString(),
                  MaxValue == Today ? DateTime.Now.ToShortDateString() : MaxValue ?? DateTime.MaxValue.ToShortDateString())
        {
            // format the default error message depending on the dates passed in. If they've customized the error message it should not override it.
            if (ErrorMessage == null)
            {
                string minText = "";
                if (MinValue == "Today")
                {
                    minText = "today's date";
                }
                else if (DateTime.TryParse(MinValue, out DateTime minDate))
                {
                    minText = minDate.ToString("MM-dd-yyyy");
                }
                string maxText = "";
                if (MaxValue == "Today")
                {
                    maxText = "today's date";
                }
                else if (DateTime.TryParse(MaxValue, out DateTime maxDate))
                {
                    maxText = maxDate.ToString("MM-dd-yyyy");
                }

                if (MinValue == null)
                {
                    ErrorMessage = string.Format(ValidationConstants.DateBeforeMessage, maxText);
                }
                else if (MaxValue == null)
                {
                    ErrorMessage = string.Format(ValidationConstants.DateAfterMessage, minText);
                }
                else if (MinValue == EarliestDefaultDate && MaxValue == LatestDefaultDate)
                {
                    ErrorMessage = ValidationConstants.DateBetween1900_2050Message;
                }
                else
                {
                    ErrorMessage = string.Format(ValidationConstants.DateBetweenMessage, minText, maxText);
                }
            }
        }
    }
}
