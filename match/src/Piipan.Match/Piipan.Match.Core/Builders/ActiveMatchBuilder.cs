using Piipan.Match.Api.Models;
using Piipan.Match.Core.Models;
using Piipan.Participants.Api.Models;
using Newtonsoft.Json;
using Piipan.Shared.API.Enums;

namespace Piipan.Match.Core.Builders
{
    /// <summary>
    /// Builder for creating IMatchDbo objects from match events data
    /// </summary>
    public class ActiveMatchBuilder : IActiveMatchBuilder
    {
        private MatchDbo _record = new MatchDbo();

        /// <summary>
        /// Initializes a new instance of ActiveMatchBuilder
        /// </summary>
        public ActiveMatchBuilder()
        {
            this.Reset();
        }

        /// <summary>
        /// Reset the builder's internal record reference
        /// </summary>
        public void Reset()
        {
            this._record = new MatchDbo();
        }

        /// <summary>
        /// Set the match record's match-related fields (Input, Data, Hash, HashType)
        /// </summary>
        /// <remarks>
        /// Currently only supports the "ldshash" hash type.
        /// </remarks>
        /// <param name="input">The RequestPerson object received as input to the active match API request.</param>
        /// <param name="match">The ParticipantRecord object received as output from active match API response.</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public IActiveMatchBuilder SetMatch(RequestPerson input, IParticipant match)
        {

            //The following line strips any current match properties before serializing and putting in a new match record
            var strippedDownRecord = new ParticipantMatch(match);
            strippedDownRecord.MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.NEWMATCH);

            this._record.Input = JsonConvert.SerializeObject(input);
            this._record.Data = JsonConvert.SerializeObject(strippedDownRecord);

            // ldshash is currently the only hash type
            this._record.Hash = input.LdsHash;
            this._record.HashType = "ldshash";

            return this;
        }

        /// <summary>
        /// Set the match record's state-related fields (Initiator, States[])
        /// </summary>
        /// <param name="initiatingState">The two-letter postal abbreviation of the initiating state.</param>
        /// <param name="matchingState">The two-letter postal abbreviation of the matching state.</param>
        /// <returns>`this` to allow for method chanining.</returns>
        public IActiveMatchBuilder SetStates(string initiatingState, string matchingState)
        {
            this._record.States = new string[] { initiatingState, matchingState };
            this._record.Initiator = initiatingState;
            return this;
        }

        /// <summary>
        /// Get the built record and reset internal record reference.
        /// </summary>
        /// <param name="initiatingState">The two-letter postal abbreviation of the initiating state.</param>
        /// <param name="matchingState">The two-letter postal abbreviation of the matching state.</param>
        /// <returns>Current IMatchDbo instance</returns>
        public IMatchDbo GetRecord()
        {
            MatchDbo record = this._record;

            this.Reset();

            return record;
        }
    }
}
