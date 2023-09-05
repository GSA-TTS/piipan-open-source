using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.Match.Core.Builders
{
    /// <summary>
    /// Builder for MatchDetailsDto objects
    /// </summary>
    public class MatchDetailsAggregator : IMatchDetailsAggregator
    {
        /// <summary>
        /// Builds a MatchDetailsDto object from match and match_res_events data
        /// </summary>
        /// <param name="matchId">The match ID for match and related match res events</param>
        public MatchDetailsDto BuildAggregateMatchDetails(
            IMatchDbo match,
            IEnumerable<IMatchResEvent> match_res_events
        )
        {
            match_res_events = match_res_events.OrderBy(e => e.InsertedAt);
            string jsonString = MergeEvents(match_res_events);
            var aggregate = JsonConvert.DeserializeObject<MatchDetailsDto>(jsonString);
            aggregate.Dispositions = AggregateDispositions(match, match_res_events);
            aggregate.Participants = CollectParticipantData(match);
            aggregate.MatchId = match.MatchId;
            aggregate.States = match.States;
            aggregate.CreatedAt = match.CreatedAt;
            aggregate.Initiator = match.Initiator;
            return aggregate;
        }

        private Disposition[] AggregateDispositions(
            IMatchDbo match,
            IEnumerable<IMatchResEvent> match_res_events
        )
        {
            var collect = new List<Disposition>();

            foreach (var stateAbbr in match.States)
            {
                var mreFiltered = match_res_events.Where(mre => mre.ActorState == stateAbbr);
                string jsonString = MergeEvents(mreFiltered, match.Initiator == stateAbbr ? match.Input : match.Data);

                var stateObj = JsonConvert.DeserializeObject<Disposition>(jsonString);

                stateObj.State = stateAbbr;
                collect.Add(stateObj);
            }
            return collect.ToArray();
        }

        private string MergeEvents(IEnumerable<IMatchResEvent> match_res_events, string originalMatchDetails = null)
        {
            return match_res_events
                .Select(mre => JObject.Parse(mre.Delta))
                .Aggregate(JObject.Parse(originalMatchDetails ?? @"{}"), (acc, x) =>
                {
                    acc.Merge(x, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Union,
                        MergeNullValueHandling = MergeNullValueHandling.Merge,
                    });
                    return acc;
                })
                .ToString();
        }

        private Participant[] CollectParticipantData(IMatchDbo match)
        {
            var collect = new List<Participant>();
            var data = JsonConvert.DeserializeObject<Participant>(match.Data);
            collect.Add(data);
            var input = JsonConvert.DeserializeObject<Participant>(match.Input);
            input.State = match.Initiator; // data has a State property, but input doesn't
            collect.Add(input);
            return collect.ToArray();
        }
    }
}
