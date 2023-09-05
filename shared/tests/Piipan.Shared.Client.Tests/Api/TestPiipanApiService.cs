using Piipan.Shared.Client.Api;

namespace Piipan.Shared.Client.Tests.Api
{
    internal class TestPiipanApiService : PiipanApiService
    {
        public TestPiipanApiService(HttpClient httpClient) : base(httpClient) { }
    }
}
