using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Database;
using Piipan.Shared.TestFixtures;
using Piipan.Shared.Tests.Mocks;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Xunit;

namespace Piipan.States.Core.Integration.Tests
{
    [Collection("Core.IntegrationTests")]
    public class StateInfoDaoTests : StateInfoDbFixture
    {
        public StateInfoDaoTests()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        private ILogger<StateInfoDao> Logger()
        {
            return new Mock<ILogger<StateInfoDao>>().Object;
        }

        [Theory]
        [InlineData("Wrong_Abbreviation", true)]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("\"", true)]
        [InlineData("\"DROP TABLE state_info\"", true)]
        [InlineData("TT", false)]
        public async void GetStateByStateAbbreviationTest(string wrongAbbreviation, bool isWrongAbbreviation)
        {
            //Arrange
            var daoStateInfo = new StateInfoDao(DbManager());
            var stateInfoDbo = StateInfoDtos.First();

            await daoStateInfo.UpsertState(stateInfoDbo);

            var dao = new StateInfoDao(DbManager());

            // Act
            var result = await dao.GetStateByAbbreviation(wrongAbbreviation);

            // Assert
            if (isWrongAbbreviation)
            {
                Assert.Null(result);
            }
            else
            {
                var storedId = stateInfoDbo.Id;
                var storedStateName = stateInfoDbo.State;
                var storedEmail = stateInfoDbo.Email;

                Assert.Equal(storedId, result.Id);
                Assert.Equal(storedStateName, result.State);
                Assert.Equal(storedEmail, result.Email);
            }
        }

        [Fact]
        public async void GetStatesTest()
        {
            // Arrange
            ClearStates();
            var daoStateInfo = new StateInfoDao(DbManager());
            foreach (var stateInfo in StateInfoDtos)
            {
                await daoStateInfo.UpsertState(stateInfo);
            }

            string[] expected = { "", "" };
            expected[0] = StateInfoDtos[1].Id;
            expected[1] = StateInfoDtos[0].Id;
            var count = 0;

            var dao = new StateInfoDao(DbManager());

            // Act
            var result = await dao.GetStates();

            // Assert
            foreach (IState state in result)
            {
                Assert.Equal(expected[count], state.Id);
                count++;
            }
        }

        [Fact]
        public async void GetEmptyStatesTest()
        {
            // Arrange
            ClearStates();

            var dao = new StateInfoDao(DbManager());

            // Act
            var result = await dao.GetStates();

            // Assert
            Assert.Empty(result);
        }
    }
}
