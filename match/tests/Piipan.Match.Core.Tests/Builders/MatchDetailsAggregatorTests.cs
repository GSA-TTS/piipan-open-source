using System;
using System.Collections.Generic;
using System.Linq;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.Models;
using Piipan.Match.Core.Services;
using Piipan.Shared.API.Utilities;
using Xunit;

namespace Piipan.Match.Core.Tests.Builders
{
    public class MatchDetailsAggregatorTests
    {
        public string _matchId = new MatchIdService().GenerateId();

        public MatchDbo _match { get; set; }

        public List<MatchResEventDbo> _match_res_events { get; set; }

        public MatchDetailsAggregatorTests()
        {
            _match = new MatchDbo()
            {
                MatchId = _matchId,
                States = new string[] { "ea", "eb" },
                Initiator = "ea",
                Input = "{\"CaseId\": \"ABC\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"DEF\"}",
                Data = "{\"State\": \"eb\", \"CaseId\": \"GHI\", \"LdsHash\": \"foobar\", \"ParticipantId\": \"JKL\", \"ParticipantClosingDate\": \"2021-02-28\", \"VulnerableIndividual\": true, \"RecentBenefitIssuanceDates\": [{\"start\":\"2021-03-01\",\"end\":\"2021-03-31\"}]}"
            };

            _match_res_events = new List<MatchResEventDbo>()
            {
                new MatchResEventDbo()
                {
                    Id = 123,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 01, 01),
                    Actor = "ea@email.example",
                    ActorState = "ea",
                    Delta = "{ initial_action_at: '2022-01-01T00:00:00.0000000' }"
                },
                new MatchResEventDbo()
                {
                    Id = 124,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 01, 02),
                    Actor = "ea@email.example",
                    ActorState = "ea",
                    Delta = "{ 'final_disposition': 'final disposition for ea' }"
                },
                new MatchResEventDbo()
                {
                    Id = 456,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 01),
                    Actor = "eb@email.example",
                    ActorState = "eb",
                    Delta = "{ 'invalid_match': false, initial_action_at: '2022-02-01T00:00:00.0000000' }"
                },
                new MatchResEventDbo()
                {
                    Id = 125,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 03),
                    Actor = "eb@email.example",
                    ActorState = "eb",
                    Delta = "{ 'invalid_match': true, 'final_disposition': 'final disposition for eb' }"
                },
                new MatchResEventDbo()
                {
                    Id = 789,
                    MatchId = _matchId,
                    InsertedAt = new DateTime(2022, 02, 04),
                    Actor = "system",
                    ActorState = null,
                    Delta = "{ 'status': 'closed' }"
                }
            };
        }

        public MatchDetailsAggregator Builder()
        {
            return new MatchDetailsAggregator();
        }

        // returns correct match status
        [Fact]
        public void Build_ReturnsCorrectStatus()
        {
            // Act
            var result = Builder().BuildAggregateMatchDetails(_match, _match_res_events);

            // Assert
            Assert.Equal("closed", result.Status);
        }
        // returns correct order
        [Fact]
        public void Build_ReturnsCorrectOrder()
        {
            // Act
            var reversed = new List<MatchResEventDbo>(_match_res_events);
            reversed = Enumerable.Reverse(reversed).ToList();
            var result = Builder().BuildAggregateMatchDetails(_match, reversed);

            // Assert
            Assert.True(result.Dispositions[1].InvalidMatch);
        }
        // returns correct state aggregate data
        // final disposition, etc for each property
        [Fact]
        public void Build_ReturnsCorrectStateData()
        {
            // Act
            var result = Builder().BuildAggregateMatchDetails(_match, _match_res_events);
            var eaObj = result.Dispositions[0];
            var ebObj = result.Dispositions[1];

            // Assert
            // Length
            Assert.Equal(2, result.Dispositions.Count());

            // state name
            Assert.Equal("ea", eaObj.State);
            Assert.Equal("eb", ebObj.State);

            // initial action at
            Assert.Equal(new DateTime(2022, 01, 01), eaObj.InitialActionAt);
            Assert.Equal(new DateTime(2022, 02, 01), ebObj.InitialActionAt);

            // invalid match
            Assert.Null(eaObj.InvalidMatch);
            Assert.True(ebObj.InvalidMatch);

            // final disposition
            Assert.Equal("final disposition for ea", eaObj.FinalDisposition);
            Assert.Equal("final disposition for eb", ebObj.FinalDisposition);

            // protect location
            Assert.Null(eaObj.VulnerableIndividual);
            Assert.Null(ebObj.VulnerableIndividual);
        }

        [Fact]
        public void Build_ReturnsCorrectParticipantData()
        {
            // Act
            var result = Builder().BuildAggregateMatchDetails(_match, _match_res_events);
            var ebParticipant = result.Participants[0];
            var eaParticipant = result.Participants[1];

            // Assert
            Assert.Equal(2, result.Participants.Length);

            // Assert ea
            Assert.Equal("ABC", eaParticipant.CaseId);
            Assert.Equal("DEF", eaParticipant.ParticipantId);
            // NOTE: initiator ParticipantClosingDate and RecentBenefitIssuanceDates
            // are not represented in the match data yet

            // Assert eb
            Assert.Equal("GHI", ebParticipant.CaseId);
            Assert.Equal("JKL", ebParticipant.ParticipantId);
            Assert.Equal(new DateTime(2021, 2, 28), ebParticipant.ParticipantClosingDate);
            Assert.Single(ebParticipant.RecentBenefitIssuanceDates);
            Assert.Collection(ebParticipant.RecentBenefitIssuanceDates,
                item => item.Equals(new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 31)))
            );
        }

    }
}
