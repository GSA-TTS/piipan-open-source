using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Dashboard.Extensions;
using Piipan.Metrics.Api;
using Xunit;

namespace Piipan.Dashboard.Tests.Extensions
{
    public class ParticipantUploadExtensionsTests
    {
        [Fact]
        public void RelativeUploadedAt_ReturnsExpected()
        {
            // Arrange
            var upload = new ParticipantUpload
            {
                State = "ea",
                UploadedAt = DateTime.Now
            };

            // Act
            var result = upload.RelativeUploadedAt();

            // Assert
            Assert.True(result.Contains("ago") || result.Contains("yesterday"));
        }

        [Theory]
        [InlineData(2021, 5, 1, 12, 0, 0, "05/01/2021 8:00AM EST")]
        [InlineData(2021, 2, 1, 19, 0, 0, "02/01/2021 2:00PM EST")]
        public void FormattedUploadedAt_ReturnsExpected(int y, int M, int d, int h, int m, int s, string expected)
        {
            // Arrange
            var upload = new ParticipantUpload
            {
                State = "ea",
                UploadedAt = new DateTime(y, M, d, h, m, s)
            };

            // Act
            var result = upload.FormattedUploadedAt();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}