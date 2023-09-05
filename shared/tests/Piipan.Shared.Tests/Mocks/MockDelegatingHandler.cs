using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Piipan.Shared.Tests.Mocks
{
    public class MockDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
