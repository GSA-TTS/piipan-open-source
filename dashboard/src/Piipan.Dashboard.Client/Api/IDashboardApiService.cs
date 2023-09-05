using Piipan.Dashboard.Client.DTO;
using Piipan.Metrics.Api;
using Piipan.Shared.Client.Api;

namespace Piipan.Dashboard.Client.Api
{
    public interface IDashboardApiService
    {
        Task<ApiResponse<ParticipantUploadStatistics>> GetUploadStatistics(ParticipantUploadStatisticsRequest request);
        Task<ApiResponse<UploadResponseDto>> GetUploads(ParticipantUploadRequestFilter request);
    }
}
