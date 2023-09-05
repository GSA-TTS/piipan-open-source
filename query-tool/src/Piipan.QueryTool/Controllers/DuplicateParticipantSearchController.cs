using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piipan.Match.Api;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Deidentification;
using Piipan.Shared.Web;

namespace Piipan.QueryTool.Controllers
{
    /// <summary>
    /// The Duplicate Participant Search Controller is used by the QueryTool UI to search for matches.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DuplicateParticipantSearchController : BaseController<DuplicateParticipantSearchController>
    {
        private readonly ILdsDeidentifier _ldsDeidentifier;
        private readonly IMatchSearchApi _matchApi;
        private const string QueryFormDataPrefix = "QueryFormData.";

        public DuplicateParticipantSearchController(
                          ILdsDeidentifier ldsDeidentifier,
                          IMatchSearchApi matchApi,
                          IServiceProvider serviceProvider)
                          : base(serviceProvider)
        {
            _ldsDeidentifier = ldsDeidentifier;
            _matchApi = matchApi;
        }

        #region Controller Actions
        /// <summary>
        /// This method performs a search given the query produced from the web form
        /// </summary>
        [HttpPost]
        public async Task<ApiResponse<OrchMatchResponseData>> PerformSearch([FromBody] DuplicateParticipantQuery queryFormModel)
        {
            RegisterExceptionHandler<ArgumentException>(new()
            {
                ExceptionHandlerCallback = (ex) =>
                {
                    if (ex.Message.ToLower().Contains("gregorian"))
                    {
                        ApiResponseContext.AddError("Date of birth must be a real date.");
                    }
                    else
                    {
                        ApiResponseContext.AddError(ex.Message);
                    }
                    return Task.CompletedTask;
                }
            });
            return await CreateApiResponse<OrchMatchResponseData>(async (response) =>
            {
                // Only locations with length 2, which are states, can perform a search.
                if (_webAppDataServiceProvider.Location.Length != 2 || _webAppDataServiceProvider.States?.Length != 1)
                {
                    response.AddError("Search performed with an invalid location");
                }
                else if (ModelState.IsValid)
                {
                    await PerformSearchForValidModel(queryFormModel, response);
                }
                else
                {
                    HandleErrorsForInvalidModel(response);
                }
            }, "There was an error running your search. Please try again.");
        }

        #endregion Controller Actions

        #region Private Methods

        private async Task PerformSearchForValidModel(DuplicateParticipantQuery queryFormModel, ApiResponse<OrchMatchResponseData> apiResponse)
        {
            _logger.LogInformation("Query form submitted");

            string digest = _ldsDeidentifier.Run(
                queryFormModel.LastName,
                queryFormModel.DateOfBirth.Value.ToString("yyyy-MM-dd"),
                queryFormModel.SocialSecurityNum
            );

            var request = new OrchMatchRequest
            {
                Data = new List<RequestPerson>
                    {
                        new RequestPerson
                        {
                            LdsHash = digest,
                            CaseId = queryFormModel.CaseId,
                            ParticipantId = queryFormModel.ParticipantId,
                            SearchReason = queryFormModel.SearchReason
                        }
                    }
            };

            var response = await _matchApi.FindAllMatches(request, _webAppDataServiceProvider.States[0].ToLower());
            apiResponse.Value = response?.Data;
        }

        private void HandleErrorsForInvalidModel(ApiResponse<OrchMatchResponseData> apiResponse)
        {
            var keys = ModelState.Keys;
            foreach (var key in keys)
            {
                if (ModelState[key]?.Errors?.Count > 0)
                {
                    var error = ModelState[key].Errors[0];
                    apiResponse.AddError(error.ErrorMessage, QueryFormDataPrefix + key);
                }
            }
        }

        #endregion Private Methods
    }
}
