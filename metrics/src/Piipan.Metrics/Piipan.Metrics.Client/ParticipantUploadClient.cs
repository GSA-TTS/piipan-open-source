using System.Threading.Tasks;
using Piipan.Metrics.Api;
using Piipan.Shared.Http;

#nullable enable

namespace Piipan.Metrics.Client
{
    public class ParticipantUploadClient : IParticipantUploadReaderApi
    {
        private readonly IAuthorizedApiClient<ParticipantUploadClient> _apiClient;

        public ParticipantUploadClient(IAuthorizedApiClient<ParticipantUploadClient> apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ParticipantUploadStatistics> GetUploadStatistics(ParticipantUploadStatisticsRequest request)
        {
            return await _apiClient.GetAsync<ParticipantUploadStatistics, ParticipantUploadStatisticsRequest>("GetUploadStatistics", request);
        }

        public async Task<GetParticipantUploadsResponse> GetLatestUploadsByState()
        {
            return await _apiClient.GetAsync<GetParticipantUploadsResponse>("GetLastUpload");
        }

        public async Task<GetParticipantUploadsResponse> GetUploads(ParticipantUploadRequestFilter filter)
        {
            return await _apiClient.GetAsync<GetParticipantUploadsResponse, ParticipantUploadRequestFilter>("GetParticipantUploads", filter);
        }
    }
}
