using Piipan.Shared.Database;
using Piipan.Shared.TestFixtures;
using Piipan.Shared.Tests.Mocks;
using Piipan.States.Core.DataAccessObjects;
using Xunit;

namespace Piipan.States.Core.Integration.Tests
{
    [Collection("Core.IntegrationTests")]
    public class StateInfoDbFixtureTests
    {
        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(Environment.GetEnvironmentVariable("CollaborationDatabaseConnectionString"),
                DefaultMocks.MockAzureServiceTokenProvider().Object);
        }

        [Fact]
        public async void StateInfoDbFixture_ShouldInitializeDatabase()
        {
            // Arrange
            var dao = new StateInfoDao(DbManager());

            //Act
            var stateInfoDbFixture = new StateInfoDbFixture();

            // Assert
            var res = await dao.GetStates();
            Assert.True(res.Count() == 1);
            Assert.True(res.First().Id == "0");

            stateInfoDbFixture.Dispose();
        }

        [Fact]
        public async void StateInfoDbFixture_ShouldCleanUpStateInfoTable()
        {
            // Arrange
            var dao = new StateInfoDao(DbManager());
            var stateInfoDbFixture = new StateInfoDbFixture();

            //Act
            stateInfoDbFixture.Dispose();

            // Assert
            var res = await dao.GetStates();

            Assert.NotNull(res);
            Assert.True(res.Count() == 0);
        }
    }
}
