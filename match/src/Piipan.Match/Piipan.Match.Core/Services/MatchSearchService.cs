using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Extensions;
using Piipan.Participants.Api;
using System;
using Piipan.Shared.Cryptography;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for discovering participant matches between states
    /// </summary>
    public class MatchSearchService : IMatchSearchApi
    {
        private readonly IParticipantApi _participantApi;
        private readonly IValidator<RequestPerson> _requestPersonValidator;
        private readonly ICryptographyClient _cryptographyClient;
        /// <summary>
        /// Initializes a new instance of MatchSearchService
        /// </summary>
        public MatchSearchService(
            IParticipantApi participantApi,
            IValidator<RequestPerson> requestPersonValidator,
            ICryptographyClient cryptographyClient)
        {
            _participantApi = participantApi;
            _requestPersonValidator = requestPersonValidator;
            _cryptographyClient = cryptographyClient;
        }

        /// <summary>
        /// Finds and returns matches for each participant in the request
        /// </summary>
        /// <param name="request">A collection of participants to attempt to find matches for</param>
        /// <returns>A collection of match results and inline errors for malformed participant requests</returns>
        public async Task<OrchMatchResponse> FindAllMatches(OrchMatchRequest request, string initiatingState)
        {
            var response = new OrchMatchResponse();
            for (int i = 0; i < request.Data.Count; i++)
            {
                request.Data[i].SearchReason = request.Data[i].SearchReason.ToString().ToLower();
                var person = request.Data[i];
                var personValidation = await _requestPersonValidator.ValidateAsync(person);
                if (personValidation.IsValid)
                {
                    var result = await CheckForAnyPersonMatches(request.Data[i], i, initiatingState);
                    response.Data.Results.Add(result);
                }
                else
                {
                    response.Data.Errors.AddRange(personValidation.Errors.Select(e =>
                    {
                        return new OrchMatchError
                        {
                            Index = i,
                            Code = e.ErrorCode,
                            Detail = e.ErrorMessage
                        };
                    }));
                }
            }

            return response;
        }

        private async Task<OrchMatchResult> CheckForAnyPersonMatches(RequestPerson person, int index, string initiatingState)
        {
            var states = await _participantApi.GetStates();
            //Encrypt hash
            person.LdsHash = _cryptographyClient.EncryptToBase64String(person.LdsHash);
            //Removing the initiatingState from the list of states for Match.
            states = states.Where(state => state != initiatingState);

            var matches = (await states
                .SelectManyAsync(state => _participantApi.GetParticipants(state, person.LdsHash)))
                .Select(p => new ParticipantMatch(p));
            return new OrchMatchResult
            {
                Index = index,
                Matches = matches
            };
        }
    }
}
