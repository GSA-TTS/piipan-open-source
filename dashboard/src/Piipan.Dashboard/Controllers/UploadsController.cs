using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Piipan.Dashboard.Client.DTO;
using Piipan.Metrics.Api;
using Piipan.Shared.Client.Api;
using Piipan.Shared.Web;

namespace Piipan.Dashboard.Controllers
{
    /// <summary>
    /// The Uploads Controller is used by the Dashboard UI to potentially call CRUD operations relating to participant uploads.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController : BaseController<UploadsController>
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;
        public UploadsController(IParticipantUploadReaderApi participantUploadApi,
            IServiceProvider serviceProvider)
                            : base(serviceProvider)

        {
            _participantUploadApi = participantUploadApi;
        }

        /// <summary>
        /// Gets all of the uploads that match the requested filter, or at least as many as fit on the page.
        /// </summary>
        /// <param name="filter">The request filter passed down to be used when querying for uploads</param>
        /// <returns>An ApiResponse object that contains the list of uploads and other metadata</returns>
        [HttpGet]
        public async Task<ApiResponse<UploadResponseDto>> GetUploads([FromQuery] ParticipantUploadRequestFilter filter)
        {
            RegisterExceptionHandler<HttpRequestException>(new()
            {
                DisplayToUserMessage = "There was an error loading data. You may be able to try again. If the problem persists, please contact system maintainers.",
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });
            return await CreateApiResponse<UploadResponseDto>(async (response) =>
            {
                if (ModelState.IsValid)
                {
                    UploadResponseDto responseData = new();
                    var getUploadsResult = await _participantUploadApi.GetUploads(filter);
                    responseData.ParticipantUploadResults = getUploadsResult.Data?.ToList();
                    responseData.PageParams = getUploadsResult.Meta.PageQueryParams;
                    responseData.Total = getUploadsResult.Meta.Total;
                    response.Value = responseData;
                }
                else
                {
                    response.Errors.AddRange(GetModelStateErrors(ModelState, "UploadRequest").Select(n => new ServerError(n.Property, n.ErrorMessage)));
                }
            });
        }

        /// <summary>
        /// Gets the total uploads that succeeded and failed that match the requested filter.
        /// </summary>
        /// <param name="filter">The request statistics filter to be used when querying for upload statistics</param>
        /// <returns>An ApiResponse object that contains the total number of success and error uploads for this request</returns>
        [HttpGet("Statistics")]
        public async Task<ApiResponse<ParticipantUploadStatistics>> GetUploadStatistics([FromQuery] ParticipantUploadStatisticsRequest request)
        {
            return await CreateApiResponse<ParticipantUploadStatistics>(async (response) =>
            {
                if (ModelState.IsValid)
                {
                    request.StartDate ??= DateTime.Now.Date;
                    request.EndDate ??= DateTime.Now.Date;
                    response.Value = await _participantUploadApi.GetUploadStatistics(request);
                }
                else
                {
                    response.Errors.AddRange(GetModelStateErrors(ModelState, "UploadRequest").Select(n => new ServerError(n.Property, n.ErrorMessage)));
                }
            });
        }
    }
}
