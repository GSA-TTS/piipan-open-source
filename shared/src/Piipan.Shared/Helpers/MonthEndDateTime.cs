using System;

namespace Piipan.Shared.Helpers
{
	public class MonthEndDateTime
	{
		/// <summary>
		/// Parse any stringified date to return last day of month
		/// </summary>
		public static DateTime Parse(string input)
		{
			DateTime dateTime = DateTime.Parse(input);
			return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
		}
	}
}
