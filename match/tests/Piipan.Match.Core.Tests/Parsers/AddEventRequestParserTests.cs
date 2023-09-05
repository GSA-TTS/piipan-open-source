using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Piipan.Shared.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Piipan.Match.Core.Tests.Parsers
{
    public class AddEventRequestParserTests
    {
        [Fact]
        public async Task EmptyStreamThrows()
        {
            // Arrange
            var logger = Mock.Of<ILogger<AddEventRequestParser>>();
            var validator = Mock.Of<IValidator<AddEventRequest>>();
            var parser = new AddEventRequestParser(validator, logger);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => BuildAndParseJson(parser, ""));
        }

        [Fact]
        public async Task ThrowsWhenValidatorThrows()
        {
            // Arrange
            var body = @"{'data':
                { 'vulnerable_individual':true }
            }";

            var logger = Mock.Of<ILogger<AddEventRequestParser>>();
            var validator = new Mock<IValidator<AddEventRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<AddEventRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure("property", "missing")
                    }));

            var parser = new AddEventRequestParser(validator.Object, logger);

            // Act / Assert
            await Assert.ThrowsAsync<ValidationException>(() => BuildAndParseJson(parser, body));
        }

        [Fact]
        public async Task ParsesObject_AndReturnsRawJObject_WhenNotAllValuesFilledIn()
        {
            // Arrange
            var body = @"{'data':
                { 'vulnerable_individual':true }
            }";

            var logger = Mock.Of<ILogger<AddEventRequestParser>>();
            var validator = new Mock<IValidator<AddEventRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<AddEventRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var parser = new AddEventRequestParser(validator.Object, logger);

            // Act
            var request = await BuildAndParseJson(parser, body);

            // Assert
            Assert.NotNull(request.ParsedRequest);
            Assert.Equal(true, request.ParsedRequest.Data.VulnerableIndividual);
            Assert.Null(request.ParsedRequest.Data.FinalDispositionDate);

            // Assert raw object contains properties passed into API, and does not contain properties not passed in
            Assert.False((request.RawRequest["data"] as Newtonsoft.Json.Linq.JObject).ContainsKey("final_disposition_date"));
            Assert.True((request.RawRequest["data"] as Newtonsoft.Json.Linq.JObject).ContainsKey("vulnerable_individual"));
        }

        private async Task<(AddEventRequest ParsedRequest, JObject RawRequest)> BuildAndParseJson(AddEventRequestParser parser, string requestBody)
        {
            using var stream = BuildStream(requestBody);
            return await parser.Parse(stream);
        }

        private Stream BuildStream(string s)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(s);
            sw.Flush();

            ms.Position = 0;

            return ms;
        }
    }
}
