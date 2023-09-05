using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Database;
using Piipan.Shared.TestFixtures;
using Piipan.Shared.Tests.Mocks;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;
using Xunit;

namespace Piipan.States.Core.Integration.Tests
{
    [Collection("Core.IntegrationTests")]
    public class StateInfoServiceTest : StateInfoDbFixture
    {
        private IDatabaseManager<CoreDbManager> DbManager()
        {
            return new SingleDatabaseManager<CoreDbManager>(ConnectionString, DefaultMocks.MockAzureServiceTokenProvider().Object);
        }
        private ILogger<StateInfoDao> Logger()
        {
            return new Mock<ILogger<StateInfoDao>>().Object;
        }

        [Fact]
        public async void GetDecryptedStateByStateAbbreviationTest()
        {
            // Arrange
            var daoStateInfo = new StateInfoDao(DbManager());
            var stateInfoDbo = StateInfoDtos.First();

            await daoStateInfo.UpsertState(stateInfoDbo);

            var expectedId = stateInfoDbo.Id;
            var expectedStateName = stateInfoDbo.State;
            var storedEmail = stateInfoDbo.Email;

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var stateInfoService = new StateInfoService(new StateInfoDao(DbManager()), cryptographyClient, Logger());

            var expectedEmail = cryptographyClient.DecryptFromBase64String(storedEmail);

            // Act
            var result = await stateInfoService.GetDecryptedState("TT");

            // Assert
            Assert.Equal(expectedStateName, result.State);
            Assert.Equal(expectedEmail, result.Email);
            Assert.Equal(expectedId, result.Id);
        }

        [Fact]
        public async void GetDecryptedStateByNameSkipsDecryptionWhenItFailsTest()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();

            var daoStateInfo = new StateInfoDao(DbManager());
            await daoStateInfo.UpsertState(InvalidStateInfoDto);
            var expected = InvalidStateInfoDto.Email;

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var stateInfoService = new StateInfoService(new StateInfoDao(DbManager()), cryptographyClient, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedState("TT");

            // Assert
            Assert.Equal(expected, result.Email);

            var state = "test";

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt Email for {state}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            logger.Verify(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt Phone for {state}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt EmailCC for {state}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        [Fact]
        public async void GetDecryptedStatesTest()
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

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var stateInfoService = new StateInfoService(new StateInfoDao(DbManager()), cryptographyClient, Logger());

            // Act
            var result = await stateInfoService.GetDecryptedStates();

            // Assert
            foreach (IState state in result)
            {
                var id = expected[count];
                Assert.Equal(id, state.Id);
                count++;
            }
        }


        [Fact]
        public async void GetDecryptedStateWithBadStateAbbreviationTest()
        {
            // Arrange
            var daoStateInfo = new StateInfoDao(DbManager());
            var stateInfoDbo = StateInfoDtos.First();

            await daoStateInfo.UpsertState(stateInfoDbo);

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var stateInfoService = new StateInfoService(new StateInfoDao(DbManager()), cryptographyClient, Logger());

            // Act
            var result = await stateInfoService.GetDecryptedState("TTT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async void GetDecryptedStatesTestEmpty()
        {
            // Arrange
            ClearStates();

            var key = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            var cryptographyClient = new AzureAesCryptographyClient(key);

            var stateInfoService = new StateInfoService(new StateInfoDao(DbManager()), cryptographyClient, Logger());

            // Act
            var result = await stateInfoService.GetDecryptedStates();

            // Assert
            Assert.Empty(result);
        }
    }
}
