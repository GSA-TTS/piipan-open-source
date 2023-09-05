using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Parsers;
using Moq;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Shared.Parsers;

namespace Piipan.Match.Core.Tests.Parsers
{
    public class OrchMatchRequestParserTests
    {
        [Fact]
        public async Task EmptyStreamThrows()
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator, logger);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream("")));
        }

        [Theory]
        [InlineData("{{")]
        [InlineData("<xml>")]
        [InlineData("{ data: 'foobar' }")]
        [InlineData("{ data: []}")]
        [InlineData("{ data: [{}]}")]
        [InlineData("{ data: [{ssn: '000-00-0000'}]}")]
        public async Task MalformedStreamThrows(string s)
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = Mock.Of<IValidator<OrchMatchRequest>>();
            var parser = new OrchMatchRequestParser(validator, logger);

            // Act / Assert
            await Assert.ThrowsAsync<StreamParserException>(() => parser.Parse(BuildStream(s)));
        }

        [Theory]
        [InlineData(@"{ data: [{ lds_hash: 'abc','participant_id': 'participant1','search_reason': 'other' }]}", 1)] // invalid hash, but valid request
        [InlineData(@"{ data: [{ lds_hash: '','participant_id': 'participant1','search_reason': 'other' }]}", 1)] // empty hash, but valid request
        // farrington,1931-10-13,000-12-3456
        [InlineData(@"{'data':[

            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458',  'participant_id': 'participant1',
              'search_reason': 'other'}
        ]}", 1)]
        [InlineData(@"{'data':[
            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458', 'participant_id': 'participant1',
              'search_reason': 'other' },
            { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458',  'participant_id': 'participant1',
              'search_reason': 'other' },
        ]}", 2)]
        public async Task WellFormedStreamReturnsObject(string body, int count)
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = new Mock<IValidator<OrchMatchRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<OrchMatchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var parser = new OrchMatchRequestParser(validator.Object, logger);

            // Act
            var request = await parser.Parse(BuildStream(body));

            // Assert
            Assert.NotNull(request.ParsedRequest);
            Assert.Equal(count, request.ParsedRequest.Data.Count());
            JToken.DeepEquals(JObject.Parse(body), request.RawRequest);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(null, null)]
        public async Task ValidVulnerableIndividual(bool? vulnerableStatus, bool? expectedVulnerableIndividual)
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = new Mock<IValidator<OrchMatchRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<OrchMatchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var parser = new OrchMatchRequestParser(validator.Object, logger);

            var orchMatchRequest = new OrchMatchRequest
            {
                Data = new List<RequestPerson>()
                {
                    new RequestPerson
                    {
                        LdsHash = "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458",
                        ParticipantId = "participant1",
                        SearchReason = "other",
                        VulnerableIndividual = vulnerableStatus
                    }
                }
            };

            //Act
            var request = await parser.Parse(BuildStream(JsonConvert.SerializeObject(orchMatchRequest)));

            // Assert
            Assert.NotNull(request.ParsedRequest);
            Assert.Equal(expectedVulnerableIndividual, request.ParsedRequest.Data[0].VulnerableIndividual);
            JToken.DeepEquals(JObject.Parse(JsonConvert.SerializeObject(orchMatchRequest)), request.RawRequest);
        }

        [Fact]
        public async Task ThrowsWhenValidatorThrows()
        {
            // Arrange
            var body = @"{'data':[
                { 'lds_hash':'eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458','participant_id': 'participant1',
                  'search_reason': 'other'}
            ]}";
            
            var logger = Mock.Of<ILogger<OrchMatchRequestParser>>();
            var validator = new Mock<IValidator<OrchMatchRequest>>();
            validator
                .Setup(m => m.ValidateAsync(It.IsAny<OrchMatchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
                    {
                        new ValidationFailure("property", "missing")
                    }));

            var parser = new OrchMatchRequestParser(validator.Object, logger);

            // Act / Assert
            await Assert.ThrowsAsync<ValidationException>(() => parser.Parse(BuildStream(body)));
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
