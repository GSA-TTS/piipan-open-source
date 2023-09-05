using System;
using Xunit;

namespace Piipan.Shared.Helpers.Tests
{
	public class MonthEndDateTimeTests
    {
		[Fact]
		public static void ParseReturnsLastDayOfMonth()
		{
			var monthWith31Days = "1970-01";
			Assert.Equal(31, MonthEndDateTime.Parse(monthWith31Days).Day);
			var monthWith30Days = "1970-04";
			Assert.Equal(30, MonthEndDateTime.Parse(monthWith30Days).Day);
			var februaryLeapYear = "2000-02";
			Assert.Equal(29, MonthEndDateTime.Parse(februaryLeapYear).Day);
			var februaryNonLeapYear = "2001-02";
			Assert.Equal(28, MonthEndDateTime.Parse(februaryNonLeapYear).Day);
		}
	}
}
