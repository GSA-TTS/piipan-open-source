using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Options;
using Moq;
using Piipan.Shared.Authentication;
using Xunit;

namespace Piipan.Shared.Tests.Authentication
{
    public class AzureTokenProviderTests
    {
        [Fact]
        public async Task RetrieveAsync_UsesConfiguredResourceUri()
        {
            // Arrange
            var tokenCredential = new Mock<TokenCredential>();
            tokenCredential
                .Setup(m => m.GetTokenAsync(It.IsAny<TokenRequestContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessToken("testToken", DateTimeOffset.FromUnixTimeSeconds(60)));

            var options = new Mock<IOptions<AzureTokenProviderOptions<AzureTokenProviderTests>>>();
            options
                .Setup(m => m.Value)
                .Returns(new AzureTokenProviderOptions<AzureTokenProviderTests>
                {
                    ResourceUri = "testUri"
                });

            var provider = new AzureTokenProvider<AzureTokenProviderTests>(
                tokenCredential.Object,
                options.Object);

            // Act
            var token = await provider.RetrieveAsync();

            // Assert
            Assert.Equal("testToken", token);
            tokenCredential
                .Verify(m => m.GetTokenAsync(
                    It.Is<TokenRequestContext>(c => c.Scopes.Contains("testUri")),
                    It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}