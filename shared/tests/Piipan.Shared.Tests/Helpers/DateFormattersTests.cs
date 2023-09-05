using System;
using System.Collections.Generic;
using Xunit;

namespace Piipan.Shared.Helpers.Tests
{
    public class DateFormattersTests
    {
        [Fact]
        public void FormatDatesAsPgArrayReturnsString()
        {
            var empty = new List<DateTime>();
            Assert.Equal("{}", DateFormatters.FormatDatesAsPgArray(empty));
            var singleDate = new List<DateTime>() { new DateTime(2021, 5, 1) };
            Assert.Equal("{2021-05-01}", DateFormatters.FormatDatesAsPgArray(singleDate));
            var multiDates = new List<DateTime>(){
                new DateTime(2021, 4, 1),
                new DateTime(2021, 5, 1)
            };
            Assert.Equal("{2021-05-01,2021-04-01}", DateFormatters.FormatDatesAsPgArray(multiDates));
        }

        [Fact]
        public void RelativeTimeReturnsOneMonthAgo()
        {
            var dtnow = new DateTime(2020, 12, 1);
            var dtrel = new DateTime(2020, 11, 1);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("one month ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsTwoMonthsAgo()
        {
            var dtnow = new DateTime(2020, 12, 1);
            var dtrel = new DateTime(2020, 10, 1);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("2 months ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsOneYearAgo()
        {
            var dtnow = new DateTime(2020, 12, 1);
            var dtrel = new DateTime(2019, 12, 1);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("one year ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsTwoYearsAgo()
        {
            var dtnow = new DateTime(2020, 12, 1);
            var dtrel = new DateTime(2018, 12, 1);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("2 years ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsyesterday()
        {
            var dtnow = new DateTime(2020, 12, 2);
            var dtrel = new DateTime(2020, 12, 1);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("yesterday", result);
        }

        [Fact]
        public void RelativeTimeReturnsTwoDaysAgo()
        {
            var dtnow = new DateTime(2020, 12, 2);
            var dtrel = new DateTime(2020, 11, 30);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("2 days ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsOneHourAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 0, 0);
            var dtrel = new DateTime(2020, 12, 2, 0, 0, 0);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("an hour ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsTwoHoursAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 0, 0);
            var dtrel = new DateTime(2020, 12, 1, 23, 0, 0);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("2 hours ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsOneMinuteAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 1, 0);
            var dtrel = new DateTime(2020, 12, 2, 1, 0, 0);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("a minute ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsTwoMinutesAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 2, 0);
            var dtrel = new DateTime(2020, 12, 2, 1, 0, 0);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("2 minutes ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsOneSecondAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 1, 5);
            var dtrel = new DateTime(2020, 12, 2, 1, 1, 4);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("one second ago", result);
        }

        [Fact]
        public void RelativeTimeReturnsFiveSecondsAgo()
        {
            var dtnow = new DateTime(2020, 12, 2, 1, 1, 5);
            var dtrel = new DateTime(2020, 12, 2, 1, 1, 0);
            string result = DateFormatters.RelativeTime(dtnow, dtrel);
            Assert.Equal("5 seconds ago", result);
        }
    }
}
