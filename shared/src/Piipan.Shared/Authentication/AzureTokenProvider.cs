using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Options;

namespace Piipan.Shared.Authentication
{
    public class AzureTokenProvider<T> : ITokenProvider<T>
    {
        private readonly TokenCredential _tokenCredential;
        private readonly AzureTokenProviderOptions<T> _options;

        public AzureTokenProvider(TokenCredential tokenCredential,
            IOptions<AzureTokenProviderOptions<T>> options)
        {
            _tokenCredential = tokenCredential;
            _options = options.Value;
        }

        public async Task<string> RetrieveAsync()
        {
            var context = new TokenRequestContext(new[] { _options.ResourceUri });
            var token = await _tokenCredential.GetTokenAsync(context, default(CancellationToken));

            return token.Token;
        }
    }
}