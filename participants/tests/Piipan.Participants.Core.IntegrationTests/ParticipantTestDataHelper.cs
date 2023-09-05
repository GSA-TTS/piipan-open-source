using Dapper;
using Dapper.NodaTime;
using Moq;
using Npgsql;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Models;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Database;
using Piipan.Shared.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace Piipan.Participants.Core.IntegrationTests
{
    public class ParticipantTestDataHelper
    {
       
        /// <summary>
        /// Creates a SingleDatabaseManager for the participant tests. In production it is multiple databases, but for integration tests it currently is just one.
        /// </summary>
        /// <param name="connectionString">The connection string to the test participant database</param>
        /// <returns></returns>
        public IDatabaseManager<ParticipantsDbManager> DbManager(string connectionString)
        {
            return new SingleDatabaseManager<ParticipantsDbManager>(connectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        private string RandomHashString()
        {
            SHA512 sha = SHA512Managed.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            var sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public IEnumerable<ParticipantDbo> RandomParticipants(int n, long uploadId)
        {
            var result = new List<ParticipantDbo>();

            for (int i = 0; i < n; i++)
            {
                result.Add(new ParticipantDbo
                {
                    LdsHash = RandomHashString(),
                    CaseId = Guid.NewGuid().ToString(),
                    ParticipantId = Guid.NewGuid().ToString(),
                    ParticipantClosingDate = DateTime.UtcNow.Date,
                    RecentBenefitIssuanceDates = new List<DateRange>
                    {
                       new DateRange(new DateTime(2021, 4, 1),new DateTime(2021, 5, 1)),
                       new DateRange(new DateTime(2021, 5, 1),new DateTime(2021, 6, 1))
                    },
                    VulnerableIndividual = (new Random()).Next(2) == 0,
                    UploadId = uploadId
                });
            }

            return result;
        }
    }
}