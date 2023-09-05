using System;
using System.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Xunit;

namespace Piipan.Match.Core.Tests.Builders
{
    public class ActiveMatchBuilderTests
    {
        [Fact]
        public void SetMatch_SetsHashAndHashType()
        {
            // Arrange
            var builder = new ActiveMatchBuilder();
            var hash = "foo";
            var input = new RequestPerson
            {
                LdsHash = hash
            };
            var match = new ParticipantMatch
            {
                LdsHash = hash
            };

            // Act
            var record = builder
                .SetMatch(input, match)
                .GetRecord();

            // Assert
            Assert.True(record.Hash == hash);
            Assert.True(record.HashType == "ldshash");
        }


        [Fact]
        public void SetMatch_SetsJsonfields()
        {
            // Arrange
            var builder = new ActiveMatchBuilder();
            var input = new RequestPerson
            {
                LdsHash = "foo",
                CaseId = "ABC",
                ParticipantId = "DEF"
            };
            var match = new ParticipantMatch
            {
                LdsHash = "foo",
                CaseId = "XYZ",
                ParticipantId = "TUV"
            };

            // Act
            var record = builder
                .SetMatch(input, match)
                .GetRecord();

            // Assert
            Assert.Equal("{\"lds_hash\":\"foo\",\"participant_id\":\"DEF\",\"case_id\":\"ABC\",\"vulnerable_individual\":null}", record.Input);
            Assert.Equal("{\"match_id\":null,\"state\":null,\"case_id\":\"XYZ\",\"participant_id\":\"TUV\",\"participant_closing_date\":null,\"recent_benefit_issuance_dates\":[],\"vulnerable_individual\":null,\"match_url\":null}", record.Data);

        }

        [Fact]
        public void SetStates_SetsInitiatingState()
        {
            // Arrange
            var builder = new ActiveMatchBuilder();
            var stateA = "ea";
            var stateB = "eb";

            // Act
            var record = builder
                .SetStates(stateA, stateB)
                .GetRecord();

            // Assert
            Assert.True(record.Initiator == stateA);
        }

        [Fact]
        public void SetStates_AddsStatesAsArray()
        {
            // Arrange
            var builder = new ActiveMatchBuilder();
            var states = new string[] { "ea", "eb" };

            // Act
            var record = builder
                .SetStates(states[0], states[1])
                .GetRecord();

            // Assert
            Assert.True(record.States.SequenceEqual(states));
        }

        [Fact]
        public void Builder_IsReusable()
        {
            // Arrange
            var builder = new ActiveMatchBuilder();

            // Act
            var recordA = builder.GetRecord();

            // Builder should reset internal MatchDbo
            // object after GetRecord() is called
            var recordB = builder.GetRecord();

            // Assert
            Assert.False(Object.ReferenceEquals(recordA, recordB));
        }

    }
}
