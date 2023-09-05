using System;
using System.Globalization;
using Piipan.Metrics.Api;
using Piipan.Metrics.Core.Builders;
using Xunit;

namespace Piipan.Metrics.Func.Api.Tests.Builders
{
    public class MetaBuilderTests
    {
        [Theory]
        [InlineData("EA", null, null, null)]
        [InlineData(null, "Complete", null, null)]
        [InlineData(null, null, "2022-08-31", null)]
        [InlineData(null, null, null, "2022-08-31")]
        [InlineData("EB", "Failed", null, null)]
        [InlineData(null, null, "2022-08-30", "2022-08-30")]
        [InlineData("EA", "Uploading", "2022-08-30", "2022-08-30")]
        public void Build_WithParameters(string state, string status, string startDateAsString, string endDateAsString)
        {
            // Arrange
            var builder = new MetaBuilder();
            DateTime? startDate = string.IsNullOrEmpty(startDateAsString) ? null : DateTime.ParseExact(startDateAsString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime? endDate = string.IsNullOrEmpty(endDateAsString) ? null : DateTime.ParseExact(endDateAsString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            builder.SetFilter(new ParticipantUploadRequestFilter
            {
                State = state,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                PerPage = 20,
                Page = 1,
                HoursOffset = -5
            });
            builder.SetTotal(5);

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(5, meta.Total);
            Assert.Contains("PerPage=20", meta.PageQueryParams);
            if (!string.IsNullOrEmpty(state))
            {
                Assert.Contains($"State={state}", meta.PageQueryParams);
            }
            else
            {
                Assert.DoesNotContain("State=", meta.PageQueryParams);
            }
            if (!string.IsNullOrEmpty(status))
            {
                Assert.Contains($"Status={status}", meta.PageQueryParams);
            }
            else
            {
                Assert.DoesNotContain("Status=", meta.PageQueryParams);
            }
            if (!string.IsNullOrEmpty(startDateAsString))
            {
                Assert.Contains($"StartDate={startDateAsString}", meta.PageQueryParams);
            }
            else
            {
                Assert.DoesNotContain("StartDate=", meta.PageQueryParams);
            }
            if (!string.IsNullOrEmpty(endDateAsString))
            {
                Assert.Contains($"EndDate={endDateAsString}", meta.PageQueryParams);
            }
            else
            {
                Assert.DoesNotContain("EndDate=", meta.PageQueryParams);
            }
        }

        [Fact]
        public void Build_WithNoFilter_DoesNotThrowException()
        {
            // Arrange
            var builder = new MetaBuilder();

            // Act
            var meta = builder.Build();

            // Assert
            Assert.Equal(0, meta.Total);
            Assert.Equal("?PerPage=0", meta.PageQueryParams);

        }
    }
}