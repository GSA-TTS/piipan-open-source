using System.Net.Http;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Models;
using Piipan.Shared.Client.Api;

namespace Piipan.QueryTool.Client.Api
{
    /// <summary>
    /// Extending the PiipanApiService
    /// </summary>
    public partial class QueryToolApiService : PiipanApiService, IQueryToolApiService
    {
        private const string DuplicateParticipantSearchApiPath = "/api/duplicateparticipantsearch";

        public QueryToolApiService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// The API call to submit a duplicate participant search requestion
        /// </summary>
        /// <param name="query">The values to be submitted as duplicate participant criteria</param>
        /// <returns>An ApiResponse that contains any errors or matches</returns>
        public async Task<ApiResponse<OrchMatchResponseData>> SubmitDuplicateParticipantSearchRequest(DuplicateParticipantQuery query)
        {
            return await PostToApi<DuplicateParticipantQuery, OrchMatchResponseData>(DuplicateParticipantSearchApiPath, query);
        }
    }
}
