using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Core.Builders;
using Piipan.Match.Core.DataAccessObjects;
using Piipan.Metrics.Api;
using Piipan.Notification.Common.Models;
using Piipan.Notifications.Core.Services;
using Piipan.Participants.Api.Models;
using Piipan.Shared.API.Enums;
using Piipan.Shared.Extensions;
using Piipan.States.Core.Service;

namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service layer for resolving match events and match records
    /// </summary>
    public class MatchEventService : IMatchEventService
    {
        private readonly IActiveMatchBuilder _recordBuilder;
        private readonly IMatchRecordApi _recordApi;
        private readonly IMatchResEventDao _matchResEventDao;
        private readonly IMatchDetailsAggregator _matchResAggregator;
        private readonly IParticipantPublishSearchMetric _participantPublishSearchMetric;
        private readonly IParticipantPublishMatchMetric _participantPublishMatchMetric;
        private readonly IStateInfoService _stateInfoService;
        private readonly INotificationService _notificationService;

        public MatchEventService(
            IActiveMatchBuilder recordBuilder,
            IMatchRecordApi recordApi,
            IMatchResEventDao matchResEventDao,
            IMatchDetailsAggregator matchResAggregator,
            IParticipantPublishSearchMetric ParticipantPublishSearchMetric,
            IParticipantPublishMatchMetric participantPublishMatchMetric,
            IStateInfoService stateInfoService,
            INotificationService notificationService)
        {
            _recordBuilder = recordBuilder;
            _recordApi = recordApi;
            _matchResEventDao = matchResEventDao;
            _matchResAggregator = matchResAggregator;
            _participantPublishSearchMetric = ParticipantPublishSearchMetric;
            _participantPublishMatchMetric = participantPublishMatchMetric;
            _stateInfoService = stateInfoService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Evaluates each new match in the incoming `matchRespone` against any existing matching records.
        /// If an open match record for the match exists, it is reused. Else, a new match record is created.
        /// Each match is subsequently updated to include the resulting `match_id`.
        /// </summary>
        /// <param name="request">The OrchMatchRequest instance derived from the incoming match request</param>
        /// <param name="matchResponse">The OrchMatchResponse instance returned from the match API</param>
        /// <param name="initiatingState">The two-letter postal abbreviation for the state initiating the match request</param>
        /// <param name="enabledStatesList">The two-letter postal abbreviation with comma seperated for the states enabled.</param>
        /// <returns>The updated `matchResponse` object with `match_id`s</returns>
        public async Task<OrchMatchResponse> ResolveMatches(OrchMatchRequest request, OrchMatchResponse matchResponse, string initiatingState, string searchFrom, string[] enabledStatesList)
        {
            matchResponse.Data.Results = (await Task.WhenAll(matchResponse.Data.Results.Select(result =>
                ResolvePersonMatches(
                    request.Data.ElementAt(result.Index),
                    result,
                    initiatingState,
                    enabledStatesList))))
                .OrderBy(result => result.Index)
                .ToList();
            //Build Search Metrics
            ParticipantSearchMetrics participantSearchMetrics = new ParticipantSearchMetrics();
            participantSearchMetrics.Data = new List<ParticipantSearch>();
            foreach (OrchMatchResult requestPerson in matchResponse.Data.Results)
            {
                var participantUploadMetrics = new ParticipantSearch()
                {
                    State = initiatingState,
                    SearchedAt = DateTime.UtcNow,
                    SearchFrom = searchFrom,//Need to Identify Website/Api call
                    SearchReason = request.Data.ElementAt(requestPerson.Index).SearchReason,
                    MatchCreation = String.IsNullOrEmpty(String.Join(",", requestPerson.Matches.Select(p => p.MatchCreation))) ? EnumHelper.GetDisplayName(SearchMatchStatus.MATCHNOTFOUND) : String.Join(",", requestPerson.Matches.Select(p => p.MatchCreation)),
                    MatchCount = requestPerson.Matches.Count()
                };
                participantSearchMetrics.Data.Add(participantUploadMetrics);
            }
            await _participantPublishSearchMetric.PublishSearchMetrics(participantSearchMetrics);
            return matchResponse;
        }

        private async Task<OrchMatchResult> ResolvePersonMatches(RequestPerson person, OrchMatchResult result, string initiatingState, string[] enabledStatesList)
        {
            // Create a match <-> match record pairing
            var pairs = result.Matches.Select(match =>
                new
                {
                    match,
                    person,
                    record = _recordBuilder
                                .SetMatch(person, match)
                                .SetStates(initiatingState, match.State)
                                .GetRecord()
                });

            result.Matches = (await Task.WhenAll(
                pairs.Select(pair => CreateNewOrReturnExistingMatch(pair.match, pair.record, pair.person, enabledStatesList))));

            return result;
        }

        private async Task<ParticipantMatch> CreateNewOrReturnExistingMatch(IParticipant match, IMatchDbo record, RequestPerson person, string[] enabledStatesList)
        {
            var existingRecords = await _recordApi.GetRecords(record);
            ParticipantMatch participantMatch = new ParticipantMatch();
            participantMatch.MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.MATCHNOTFOUND);
            if (existingRecords.Any())
            {
                participantMatch = await DetermineIfExistingMatchIsOpen(match, record, existingRecords);
            }
            else
            {
                // No existing records
                participantMatch = new ParticipantMatch(match)
                {
                    MatchId = await _recordApi.AddNewMatchRecord(record),
                    MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.NEWMATCH)
                };
                // New Match is created.  Create new Match entry in the Metrics database
                //BuildAggregateMatchDetails Search Metrics
                var participantMatchMetrics = new ParticipantMatchMetrics()
                {
                    MatchId = participantMatch.MatchId,
                    InitState = record.Initiator,
                    MatchingState = match.State,
                    MatchingStateVulnerableIndividual = match.VulnerableIndividual,
                    InitStateVulnerableIndividual = person.VulnerableIndividual, // getting VulnerableIndividual from iniator 
                    Status = MatchRecordStatus.Open

                };
                await _participantPublishMatchMetric.PublishMatchMetric(participantMatchMetrics);


                //Send Notification to both initiating state and Matching State.
                // Send template data for any email template which is created based on the requirements.
                // The below logic might change based on the template data for requirements.
                //In future we might end up consolidating the logic based on requirements.
                var states = await _stateInfoService.GetDecryptedStates();
                var initState = states?.Where(n => string.Compare(n.StateAbbreviation, record.Initiator, true) == 0).FirstOrDefault();
                var matchingState = states?.Where(n => string.Compare(n.StateAbbreviation, match.State, true) == 0).FirstOrDefault();
                var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");

                // Send emails only if the state is enabled. nac-1902
                if (enabledStatesList.Contains(matchingState.StateAbbreviation.ToLower()) && enabledStatesList.Contains(initState.StateAbbreviation.ToLower())) //Send Email only if the Initiating state or Matching State is Enabled
                {
                    var MatchRecord = new MatchEmailModel()
                    {
                        MatchId = participantMatch.MatchId,
                        InitState = initState?.State,
                        MatchingState = matchingState?.State,
                        MatchingUrl = $"{queryToolUrl}/match/{participantMatch.MatchId}",
                        CreateDate = record.CreatedAt,
                        InitialActionBy = DateTime.UtcNow.ToEasternTime().AddDays(10), // Converting to Eastern Time since we don't know the end user's timezone. See NAC-1613
                        IsInitiatingStateEnabled = enabledStatesList.Contains(initState.StateAbbreviation.ToLower()),
                        IsMatchingStateEnabled = enabledStatesList.Contains(matchingState.StateAbbreviation.ToLower())

                    };

                    NotificationRecord notificationRecord = new NotificationRecord();
                    notificationRecord.MatchEmailDetails = new MatchEmailModel();
                    notificationRecord.MatchEmailDetails = MatchRecord;
                    notificationRecord.InitiatingStateEmailRecipientsModel = new EmailToModel();
                    notificationRecord.InitiatingStateEmailRecipientsModel.EmailTo = initState?.Email;
                    notificationRecord.InitiatingStateEmailRecipientsModel.EmailCcTo = initState?.EmailCc;
                    notificationRecord.IsInitiatingStateEnabled = enabledStatesList.Contains(initState.StateAbbreviation.ToLower());

                    notificationRecord.MatchingStateEmailRecipientsModel = new EmailToModel();
                    notificationRecord.MatchingStateEmailRecipientsModel.EmailTo = matchingState?.Email;
                    notificationRecord.MatchingStateEmailRecipientsModel.EmailCcTo = matchingState?.EmailCc;
                    notificationRecord.IsMatchingStateEnabled = enabledStatesList.Contains(matchingState.StateAbbreviation.ToLower());
                    notificationRecord.MatchEmailDetails.ReplyToEmail = Environment.GetEnvironmentVariable("SmtpReplyToEmail");
                    await _notificationService.PublishNotificationOnMatchCreation(notificationRecord); //Publishing Email for Initiating & Matching State:  Based on the requirement
                }

            }
            if (participantMatch != null)
            {
                var queryToolUrl = Environment.GetEnvironmentVariable("QueryToolUrl");
                participantMatch.MatchUrl = $"{queryToolUrl}/match/{participantMatch.MatchId}";
            }
            return participantMatch;
        }

        private async Task<ParticipantMatch> DetermineIfExistingMatchIsOpen(IParticipant match, IMatchDbo pendingRecord, IEnumerable<IMatchDbo> existingRecords)
        {
            var latest = existingRecords.OrderBy(r => r.CreatedAt).Last();

            // TODO: Think about adding a "current status" column to the match record so that we don't need to grab all of the events.
            var events = await _matchResEventDao.GetEventsByMatchId(latest.MatchId);
            var matchResRecord = _matchResAggregator.BuildAggregateMatchDetails(latest, events);

            if (matchResRecord.Status == MatchRecordStatus.Closed)
            {
                return new ParticipantMatch(match)
                {
                    MatchId = await _recordApi.AddNewMatchRecord(pendingRecord),
                    MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.NEWMATCH)
                };
            }

            // Latest record is open, return its match ID
            return new ParticipantMatch(match)
            {
                MatchId = latest.MatchId,
                MatchCreation = EnumHelper.GetDisplayName(SearchMatchStatus.EXISTINGMATCH)
            };

        }
    }
}