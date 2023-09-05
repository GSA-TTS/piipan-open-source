using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.API.Constants;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Helpers;
using Piipan.Shared.Http;
using Piipan.Shared.Web;

namespace Piipan.QueryTool.Controllers
{
    /// <summary>
    /// The Match Controller is used by the QueryTool UI to potentially call CRUD operations relating to matches.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : BaseController<MatchController>
    {
        private readonly IMatchResolutionApi _matchResolutionApi;

        public MatchController(IMatchResolutionApi matchResolutionApi
                           , IServiceProvider serviceProvider)
                          : base(serviceProvider)

        {
            _matchResolutionApi = matchResolutionApi;
        }

        #region Controller Actions
        /// <summary>
        /// Gets a match and all details by the match ID
        /// </summary>
        /// <param name="id">The ID of the match to query</param>
        /// <returns>An ApiResponse that contains the found match</returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse<MatchResApiResponse>> GetMatchDetailById([FromRoute] string id)
        {
            return await CreateApiResponse<MatchResApiResponse>(async (response) =>
            {
                if (IsValidMatchId(id))
                {
                    if (!_rolesProvider.TryGetRoles(RoleConstants.ViewMatchArea).Contains(_webAppDataServiceProvider.Role))
                    {
                        response.IsUnauthorized = true;
                        return;
                    }

                    var match = await _matchResolutionApi.GetMatch(id, _webAppDataServiceProvider.IsNationalOffice ? "*" : _webAppDataServiceProvider.Location);

                    // Match might not come back because it's either not there or we don't have access to it.
                    // Either way, mark the response as Unauthorized and the UI will handle accordingly.
                    if (match == null)
                    {
                        response.IsUnauthorized = true;
                    }
                    else
                    {
                        response.Value = match;
                    }
                }
                else
                {
                    // If the match ID is not in the correct format, we obviously did not find a match.
                    // Mark as Unauthorized and the UI will handle accordinly.
                    response.IsUnauthorized = true;
                }
            });
        }

        /// <summary>
        /// Gets all matches and their details. This should only be called from the Match List page
        /// If you are not a national office user you will get an unauthorized response.
        /// </summary>
        /// <returns>An ApiResponse that contains the list of all matches in the system</returns>
        [HttpGet]
        public async Task<ApiResponse<MatchResListApiResponse>> GetAllMatchDetails()
        {
            return await CreateApiResponse<MatchResListApiResponse>(async (response) =>
            {
                if (!_webAppDataServiceProvider.IsNationalOffice)
                {
                    response.IsUnauthorized = true;
                }
                else
                {
                    response.Value = await _matchResolutionApi.GetMatches();
                }
            });
        }

        /// <summary>
        /// This saves an update to a match, creating a new record in the match_res_events table.
        /// </summary>
        /// <param name="id">The match ID of the match being saved</param>
        /// <param name="update">The updates being applied to the match</param>
        /// <returns>An ApiResponse containing the new match post-update and any alerts triggered from saving the match.</returns>
        [HttpPost("{id}")]
        public async Task<ApiResponse<MatchDetailSaveResponse>> SaveMatchUpdate([FromRoute] string id, [FromBody] DispositionModel update)
        {
            return await CreateApiResponse<MatchDetailSaveResponse>(async (response) =>
            {
                if (!UserHasRole(RoleConstants.EditMatchArea))
                {
                    _logger.LogError($"User {_webAppDataServiceProvider.Email} does not have permissions to edit match details.");
                    response.IsUnauthorized = true;
                    response.AddError("You do not have the role and permissions to edit match details.");
                    return;
                }

                if (ModelState.IsValid)
                {
                    var matchBeforeSave = await GetMatchAndAddMatchResEvent(id, response, update);

                    if (response.IsUnauthorized)
                        return;

                    CheckSavedMatchAndStatus(matchBeforeSave, response);
                }
                else
                {
                    response.Errors.AddRange(GetModelStateErrors(ModelState, "DispositionData")
                        .Select(n => new ServerError(n.Property, n.ErrorMessage)));
                }
            }, "There was an error saving your data. Please try again.");
        }

        #endregion Controller Actions

        #region Private Methods
        /// <summary>
        /// Checks to see if the match is valid. The match is rejected if any of the following are true:
        ///   - It contains invalid characters (https://github.com/18F/piipan/pull/2692#issuecomment-1045071033)
        ///   - It is not 7 digits long (https://github.com/18F/piipan/pull/2692#issuecomment-1045071033)
        ///   - It is null
        /// </summary>
        /// <param name="matchId"></param>
        /// <returns></returns>
        private bool IsValidMatchId(string matchId)
        {
            //Prevents malicious user input
            Regex r = new Regex("^[a-zA-Z0-9]*$");

            return !string.IsNullOrWhiteSpace(matchId) && matchId.Length == 7 && r.IsMatch(matchId);
        }

        private async Task<MatchResApiResponse> GetMatchAndAddMatchResEvent(string id, ApiResponse<MatchDetailSaveResponse> response, DispositionModel update)
        {
            AddEventRequest addEventRequest = new AddEventRequest
            {
                Data = update.ToDisposition()
            };

            MatchResApiResponse matchBeforeSave = await _matchResolutionApi.GetMatch(id, _webAppDataServiceProvider.IsNationalOffice ? "*" : _webAppDataServiceProvider.Location);
            var (_, failResponse) = await _matchResolutionApi.AddMatchResEvent(id, addEventRequest, _webAppDataServiceProvider.Location);

            if (string.IsNullOrEmpty(failResponse))
            {
                await UpdateApiResponse(id, response);
            }
            else
            {
                HandleErrors(response, failResponse);
            }

            return matchBeforeSave;
        }

        private async Task UpdateApiResponse(string id, ApiResponse<MatchDetailSaveResponse> response)
        {
            // Set the response success value to true to the let the API know it was saved successfully.
            var matchAfterSave = await _matchResolutionApi.GetMatch(id, _webAppDataServiceProvider.IsNationalOffice ? "*" : _webAppDataServiceProvider.Location);

            if (matchAfterSave == null)
            {
                response.IsUnauthorized = true;
                return;
            }

            response.Value = new MatchDetailSaveResponse { SavedMatch = matchAfterSave };
            response.Value.Alerts.Add(new Alert()
            {
                Html = "<strong>Your update has been successfully saved.</strong>",
                AlertSeverity = Components.Enums.AlertSeverity.Success
            });
        }

        private void CheckSavedMatchAndStatus(MatchResApiResponse matchBeforeSave, ApiResponse<MatchDetailSaveResponse> response)
        {
            // if SavedMatch has a value, match saved successfully
            if (matchBeforeSave?.Data.Status == MatchRecordStatus.Open && response.Value?.SavedMatch?.Data.Status == MatchRecordStatus.Closed)
            {
                response.Value.Alerts.Add(new Alert()
                {
                    Html = "<strong>This match has been successfully closed.</strong>",
                    AlertSeverity = Components.Enums.AlertSeverity.Success
                });
            }
        }

        private void HandleErrors(ApiResponse<MatchDetailSaveResponse> response, string failResponse)
        {
            ApiErrorResponse apiErrorResponse = JsonHelper.TryParse<ApiErrorResponse>(failResponse);

            if (apiErrorResponse?.Errors == null || apiErrorResponse?.Errors?.Count == 0)
            {
                response.AddError("There was an error saving your data. Please try again.");
                return;
            }

            foreach (var error in apiErrorResponse?.Errors)
            {
                CheckAndWriteErrorMessage(error.Detail, response);
            }
        }

        private void CheckAndWriteErrorMessage(string errorDetail, ApiResponse<MatchDetailSaveResponse> response)
        {
            if (string.Equals(errorDetail, "Duplicate action not allowed", StringComparison.OrdinalIgnoreCase))
            {
                response.Value = new MatchDetailSaveResponse();
                response.Value.Alerts.Add(new Alert()
                {
                    Html = "<strong>There are no changes to save.</strong>",
                    AlertSeverity = Components.Enums.AlertSeverity.Info
                });
            }
            else
            {
                response.Errors.Add(new("", errorDetail));
            }
        }
        #endregion Private Methods
    }
}
