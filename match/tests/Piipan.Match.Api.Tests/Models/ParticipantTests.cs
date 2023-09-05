using Newtonsoft.Json;
using Piipan.Match.Api.Models;
using Xunit;

namespace Piipan.Match.Api.Tests.Models
{
    public class ParticipantTests
    {
        [Fact]
        public void ParticipantRecordJson()
        {
            // Arrange
            var json = @"{participant_id: 'baz', case_id: 'foo', participant_closing_date: '2020-01-12', recent_benefit_issuance_dates: [{Start: '2021-04-01', End: '2021-04-16' }, { Start: '2021-03-01', End: '2021-03-31' }, { Start: '2021-02-01', End: '2021-03-01' } 
], vulnerable_individual: true}";
            var record = JsonConvert.DeserializeObject<ParticipantMatch>(json);

            string jsonRecord = record.ToJson();

            Assert.Contains("\"state\": null", jsonRecord);
            Assert.Contains("\"participant_id\": \"baz\"", jsonRecord);
            Assert.Contains("\"case_id\": \"foo\"", jsonRecord);
            Assert.Contains("\"participant_closing_date\": \"2020-01-12\"", jsonRecord);
            Assert.Contains("\"recent_benefit_issuance_dates\": [", jsonRecord);
            Assert.Contains("\"start\": \"2021-04-01\"", jsonRecord); 
            Assert.Contains("\"start\": \"2021-03-01\"", jsonRecord);
            Assert.Contains("\"start\": \"2021-02-01\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-04-16\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-03-31\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-03-01\"", jsonRecord);
            Assert.Contains("\"vulnerable_individual\": true", jsonRecord);
        }

        [Fact]
        public void ParticipantRecordJsonIgnoresLdsHash()
        {
            // Arrange
            var json = @"{participant_id: 'baz', lds_hash: 'testingHash', case_id: 'foo', participant_closing_date: '2020-01-12', recent_benefit_issuance_dates: [{Start: '2021-04-01', End: '2021-04-16' }, { Start: '2021-03-01', End: '2021-03-31' }, { Start: '2021-02-01', End: '2021-03-01' } 
], vulnerable_individual: true}";
            var record = JsonConvert.DeserializeObject<ParticipantMatch>(json);

            string jsonRecord = record.ToJson();

            Assert.Contains("\"state\": null", jsonRecord);
            Assert.Contains("\"participant_id\": \"baz\"", jsonRecord);
            Assert.Contains("\"case_id\": \"foo\"", jsonRecord);
            Assert.Contains("\"participant_closing_date\": \"2020-01-12\"", jsonRecord);
            Assert.Contains("\"recent_benefit_issuance_dates\": [", jsonRecord);
            Assert.Contains("\"start\": \"2021-04-01\"", jsonRecord);
            Assert.Contains("\"start\": \"2021-03-01\"", jsonRecord);
            Assert.Contains("\"start\": \"2021-02-01\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-04-16\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-03-31\"", jsonRecord);
            Assert.Contains("\"end\": \"2021-03-01\"", jsonRecord);
            Assert.Contains("\"vulnerable_individual\": true", jsonRecord);
            Assert.DoesNotContain("\"lds_hash\": \"testingHash\"", jsonRecord);
        }
    }
}
