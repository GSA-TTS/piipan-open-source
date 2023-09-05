using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Shared.Cryptography;
using Piipan.States.Api.Models;
using Piipan.States.Core.DataAccessObjects;
using Piipan.States.Core.Service;
using Piipan.States.Core.Models;
using Xunit;

namespace Piipan.States.Core.Integration.Tests
{
    public class StateInfoServiceTest
    {
        private IState _stateInfoDto = new StateInfoDbo()
        {
            Id = "99",
            State = "test1",
            StateAbbreviation = "TT",
            Email = "m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj",
            Phone = "sajNo6NgQGJJ14MtpBvrCg==",
            Region = "TEST",
            EmailCc = "+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"
        };

        [Fact]
        public async void GetDecryptedStateByStateAbbreviationTest()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();
            var daoStateInfoMock = new Mock<IStateInfoDao>();
            daoStateInfoMock.Setup(c => c.GetStateByAbbreviation(It.IsAny<string>()))
                .Returns(Task.FromResult(_stateInfoDto));

            var cryptographyClient = new Mock<ICryptographyClient>();

            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Email))
                .Returns("email@email.gov");
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Phone))
                .Returns("123123123");
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.EmailCc))
                .Returns("emailCc@email.gov");

            var stateInfoService = new StateInfoService(daoStateInfoMock.Object, cryptographyClient.Object, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedState("TT");

            // Assert
            daoStateInfoMock.Verify(c => c.GetStateByAbbreviation(It.Is<string>(d => d == "TT")));
            Assert.Equal("test1", result.State);
            Assert.Equal("email@email.gov", result.Email);
            Assert.Equal("99", result.Id);
            Assert.Equal("emailCc@email.gov", result.EmailCc);
            Assert.Equal("123123123", result.Phone);

            logger.Verify(c => c.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()), Times.Never);

            cryptographyClient.Verify(c => c.DecryptFromBase64String("m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj"), Times.Once);
            Assert.Equal("email@email.gov", cryptographyClient.Object.DecryptFromBase64String("m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj"));

            cryptographyClient.Verify(c => c.DecryptFromBase64String("sajNo6NgQGJJ14MtpBvrCg=="), Times.Once);
            Assert.Equal("123123123", cryptographyClient.Object.DecryptFromBase64String("sajNo6NgQGJJ14MtpBvrCg=="));

            cryptographyClient.Verify(c => c.DecryptFromBase64String("+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"), Times.Once);
            Assert.Equal("emailCc@email.gov", cryptographyClient.Object.DecryptFromBase64String("+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"));
        }

        [Fact]
        public async void GetDecryptedStateWhenNull()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();

            var daoStateInfoMock = new Mock<IStateInfoDao>();
            daoStateInfoMock.Setup(c => c.GetStateByAbbreviation(It.IsAny<string>()));

            var cryptographyClient = new Mock<ICryptographyClient>();

            var stateInfoService = new StateInfoService(daoStateInfoMock.Object, cryptographyClient.Object, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedState("TT");

            // Assert
            daoStateInfoMock.Verify(c => c.GetStateByAbbreviation(It.Is<string>(d => d == "TT")));
            Assert.Null(result);
            logger.Verify(c => c.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()), Times.Never);

            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.Email), Times.Never);
            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.Phone), Times.Never);
            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.EmailCc), Times.Never);
        }

        [Fact]
        public async void GetDecryptedStateByNameSkipsDecryptionWhenItFailsTest()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();
            var daoStateInfoMock = new Mock<IStateInfoDao>();
            daoStateInfoMock.Setup(c => c.GetStateByAbbreviation(It.IsAny<string>()))
                .Returns(Task.FromResult(_stateInfoDto));

            var cryptographyClient = new Mock<ICryptographyClient>();
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Email))
                .Throws(new Exception(It.IsAny<string>()));
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Phone))
                .Throws(new Exception(It.IsAny<string>()));
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.EmailCc))
                .Throws(new Exception(It.IsAny<string>()));

            var stateInfoService = new StateInfoService(daoStateInfoMock.Object, cryptographyClient.Object, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedState("TT");

            // Assert
            Assert.Equal(_stateInfoDto.Email, result.Email);
            Assert.Equal(_stateInfoDto.EmailCc, result.EmailCc);
            Assert.Equal(_stateInfoDto.Phone, result.Phone);


            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt Email for {_stateInfoDto.State}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt Phone for {_stateInfoDto.State}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);

            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Failed to decrypt EmailCC for {_stateInfoDto.State}."),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);


            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.Email), Times.Once);
            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.Phone), Times.Once);
            cryptographyClient.Verify(c => c.DecryptFromBase64String(_stateInfoDto.EmailCc), Times.Once);
        }

        [Fact]
        public async void GetDecryptedStatesTest()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();
            var daoStateInfoMock = new Mock<IStateInfoDao>();
            daoStateInfoMock.Setup(c => c.GetStates())
                .Returns(Task.FromResult((new List<IState>() { _stateInfoDto }).AsEnumerable()));

            var cryptographyClient = new Mock<ICryptographyClient>();
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Email))
                .Returns("email@email.gov");
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.Phone))
                .Returns("123123123");
            cryptographyClient.Setup(c => c.DecryptFromBase64String(_stateInfoDto.EmailCc))
                .Returns("emailCc@email.gov");

            var stateInfoService = new StateInfoService(daoStateInfoMock.Object, cryptographyClient.Object, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedStates();

            // Assert
            Assert.Equal(1, result?.Count());
            Assert.Equal("email@email.gov", result?.ToArray()[0].Email);
            Assert.Equal("123123123", result?.ToArray()[0].Phone);
            Assert.Equal("emailCc@email.gov", result?.ToArray()[0].EmailCc);
            Assert.Equal(_stateInfoDto.Id, result?.ToArray()[0].Id);
            Assert.Equal(_stateInfoDto.StateAbbreviation, result?.ToArray()[0].StateAbbreviation);
            Assert.Equal(_stateInfoDto.State, result?.ToArray()[0].State);

            cryptographyClient.Verify(c => c.DecryptFromBase64String("m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj"), Times.Once);
            Assert.Equal("email@email.gov", cryptographyClient.Object.DecryptFromBase64String("m4cHpnQQ6OmkRi3MbA8nc9H/bC5eVf7uOmRhK8L7qWzKHBbQcUwd28WL40mw/BYj"));

            cryptographyClient.Verify(c => c.DecryptFromBase64String("sajNo6NgQGJJ14MtpBvrCg=="), Times.Once);
            Assert.Equal("123123123", cryptographyClient.Object.DecryptFromBase64String("sajNo6NgQGJJ14MtpBvrCg=="));

            cryptographyClient.Verify(c => c.DecryptFromBase64String("+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"), Times.Once);
            Assert.Equal("emailCc@email.gov", cryptographyClient.Object.DecryptFromBase64String("+eU5yR+Io0kIiWFnGQ2+WX4zUi/gVwcqQuKJokWYydV+zJ5HHqEZGGNqvlwKWD6/"));
        }


        [Fact]
        public async void GetDecryptedStateWithBadStateAbbreviationTest()
        {
            // Arrange
            var logger = new Mock<ILogger<StateInfoDao>>();
            var daoStateInfoMock = new Mock<IStateInfoDao>();
            daoStateInfoMock.Setup(c => c.GetStates());

            var cryptographyClient = new Mock<ICryptographyClient>();

            var stateInfoService = new StateInfoService(daoStateInfoMock.Object, cryptographyClient.Object, logger.Object);

            // Act
            var result = await stateInfoService.GetDecryptedState("TTT");

            // Assert
            Assert.Null(result);
        }
    }
}
