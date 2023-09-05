using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.API.Utilities;

namespace Piipan.QueryTool.Client.Helpers
{
    public static class ValueToDisplayHelper
    {
        public const string DateFormat = "MM/dd/yyyy";
        public static string GetDisplayValue<T>(T value, DisplayInfoType format = DisplayInfoType.None)
        {
            if (value == null)
            {
                return "-";
            }
            return value switch
            {
                bool b => b ? "Yes" : "No",
                DateTime d => d.ToString(DateFormat),
                DateRange d => DateRangeFormat(d),
                IEnumerable<DateRange> ds => ds?.Count() == 0 ? "-" : string.Join('\n', ds.Select(d => DateRangeFormat(d))),
                _ => FormatString(value.ToString(), format)
            };
        }
        private static string FormatString(string value, DisplayInfoType format)
        {
            if (format == DisplayInfoType.Phone)
            {
                string phoneFormattedValue = "";
                int lastIndex = 0;
                if (!value.Contains("-"))
                {
                    if (value.Length > 3)
                    {
                        lastIndex = 3;
                        phoneFormattedValue = value[0..3] + "-";
                    }
                    if (value.Length > 6)
                    {
                        lastIndex = 6;
                        phoneFormattedValue += value[3..6] + "-";
                    }
                    phoneFormattedValue += value[lastIndex..];
                }
                else
                {
                    phoneFormattedValue = value;
                }
                return phoneFormattedValue;
            }
            return value;
        }
        private static string DateRangeFormat(DateRange dateRange)
        {
            return $"{dateRange.Start.ToString(DateFormat)} - {dateRange.End.ToString(DateFormat)}";
        }
    }
}
