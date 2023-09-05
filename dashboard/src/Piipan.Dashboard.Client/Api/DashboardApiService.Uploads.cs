using Piipan.Dashboard.Client.DTO;
using Piipan.Metrics.Api;
using Piipan.Shared.Client.Api;

namespace Piipan.Dashboard.Client.Api
{
    /// <summary>
    /// Extending the PiipanApiService
    /// </summary>
    public partial class DashboardApiService : PiipanApiService, IDashboardApiService
    {
        private const string UploadsApiPath = "/api/uploads";
        public DashboardApiService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// The API call to submit a duplicate participant search requestion
        /// </summary>
        /// <param name="query">The values to be submitted as duplicate participant criteria</param>
        /// <returns>An ApiResponse that contains any errors or matches</returns>
        public async Task<ApiResponse<ParticipantUploadStatistics>> GetUploadStatistics(ParticipantUploadStatisticsRequest request)
        {
            return await GetFromApi<ParticipantUploadStatistics>($"{UploadsApiPath}/statistics", request);
        }

        /// <summary>
        /// The API call to submit a duplicate participant search requestion
        /// </summary>
        /// <param name="query">The values to be submitted as duplicate participant criteria</param>
        /// <returns>An ApiResponse that contains any errors or matches</returns>
        public async Task<ApiResponse<UploadResponseDto>> GetUploads(ParticipantUploadRequestFilter request)
        {
            return await GetFromApi<UploadResponseDto>(UploadsApiPath, request);
        }
    }
}
