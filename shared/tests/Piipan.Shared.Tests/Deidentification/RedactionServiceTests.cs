using System;
using System.Collections.Generic;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Shared.Tests.Deidentification
{
    public class RedactionServiceTests
    {
        private readonly RedactionService redactionService = new();

        [Theory]
        [InlineData("ZZXMMMc1g6JK0daCQ52zczbe7SmduDPjdofc0gLap64R8CKpVfKxg4ex0tCAMcR9AfrYsLOuym605E2QBaipJuq75jmhzZqj02uXZZSdoEA=", "ZZXMMMc1g6JK0daCQ52zczbe7SmduDPjdofc0gLap64R8CKpVfKxg4ex0tCAMcR9AfrYsLOuym605E2QBaipJuq75jmhzZqj02uXZZSdoEA=")]
        [InlineData("'ZZXMMMc1g6JK0daCQ52zczbe7SmduDPjdofc0gLap64R8CKpVfKxg4ex0tCAMcR9AfrYsLOuym605E2QBaipJuq75jmhzZqj02uXZZSdoEA='", "ZZXMMMc1g6JK0daCQ52zczbe7SmduDPjdofc0gLap64R8CKpVfKxg4ex0tCAMcR9AfrYsLOuym605E2QBaipJuq75jmhzZqj02uXZZSdoEA=")]
        [InlineData("\"e12067186838fa138a1c8dfd68627788821ad2512b2acc214f719d647492b5e0b6cacdd7b00f466155cce2218662d32c290d35c1ef16db9e052ef4964c860be1\"", "e12067186838fa138a1c8dfd68627788821ad2512b2acc214f719d647492b5e0b6cacdd7b00f466155cce2218662d32c290d35c1ef16db9e052ef4964c860be1")]
        [InlineData("(DHEzaRj7AMW4NpK1x5JeRsHIB8kdcR+L+ZzkVbb6gQzcBkAGdaEGdt1hhUJfBL59knd2pVraD2BXucHdg6EGHQRi3kBxGXA5zrpczec9FG5lp89MCdwDklXKRQ2fcKmvl0pyWUkGq+1pq7rzIbNWQ/y1TK/smDn/ZxYdMvhOBuXNFiWIjkcXCxXgoq1nGYQXOJkAg6GGMh/Ret0yjl0ptGwFA0+7K5tGJ/om/gpTwA6SFpWgn9SLzYaf5ARXx29/zRnw5uf3+N25ZOWRUanTdh7KjW5U5uirqcbkNd6oUNovLfX78MYM5gVcK8gAW11vjYXUFM4giqvhBOEi7iubx4CLQn38QZ7wPoNv6FHlq8I=)", "DHEzaRj7AMW4NpK1x5JeRsHIB8kdcR+L+ZzkVbb6gQzcBkAGdaEGdt1hhUJfBL59knd2pVraD2BXucHdg6EGHQRi3kBxGXA5zrpczec9FG5lp89MCdwDklXKRQ2fcKmvl0pyWUkGq+1pq7rzIbNWQ/y1TK/smDn/ZxYdMvhOBuXNFiWIjkcXCxXgoq1nGYQXOJkAg6GGMh/Ret0yjl0ptGwFA0+7K5tGJ/om/gpTwA6SFpWgn9SLzYaf5ARXx29/zRnw5uf3+N25ZOWRUanTdh7KjW5U5uirqcbkNd6oUNovLfX78MYM5gVcK8gAW11vjYXUFM4giqvhBOEi7iubx4CLQn38QZ7wPoNv6FHlq8I=")]
        [InlineData(" IC3sgrAQUCxWHKczfskTfQ== ", "IC3sgrAQUCxWHKczfskTfQ==")]
        [InlineData("[tRYAokMl4BcBwsJ1SlTt+AuqcR3ZnOw/MeUk0+p0xV8=]", "tRYAokMl4BcBwsJ1SlTt+AuqcR3ZnOw/MeUk0+p0xV8=")]
        [InlineData("{caseEA00828039}", "caseEA00828039")]
        [InlineData("<partEA00986794>", "partEA00986794")]
        public void RedactsNothingWhenNullList(string piiIntoInErrorMessage, string pii)
        {
            // Arrange
            var uploadDetails = new ParticipantUploadErrorDetails("EA", DateTime.UtcNow, DateTime.UtcNow, new Exception("Exception with first participant: " + piiIntoInErrorMessage), "test.csv");

            var piiHashSet = new HashSet<string>() { pii };

            // Act
            var redactedString = redactionService.RedactParticipantsUploadError(uploadDetails.ToString(), piiHashSet);

            // Assert
            Assert.True($"{uploadDetails.ToString().Replace(pii, "REDACTED")}" == redactedString);
        }
    }
}
