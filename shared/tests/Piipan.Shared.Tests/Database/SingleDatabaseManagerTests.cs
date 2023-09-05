using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Moq;
using Npgsql;
using Piipan.Shared.Tests.Mocks;
using Xunit;

namespace Piipan.Shared.Database.Tests
{
    /// <summary>
    /// A collection of tests that verifies the changes made to SingleDatabaseManager, including how it performs queries and creates connection strings
    /// over multiple threads.
    /// </summary>
    [Collection("Piipan.Shared.DatabaseManager")]
    public class SingleDatabaseManagerTests
    {
        //NOTE: the sample connection string below includes "Persist Security Info=true" only as a means for testing and verification.
        //      This setting should almost never be on real connection strings, otherwises passwords could leak to logs, console, etc.
        private const string ConnectionString = "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};Persist Security Info=true;";
        private const string ConnectionStringWithoutNeedingToken = "Server=server;Database=db;Port=5432;User Id=postgres;Password=example;Persist Security Info=true;";
        private struct MockType { };

        private void ClearEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable(BaseDatabaseManager<MockType>.CloudName, null);
        }

        private async Task<int> PerformDummyQuery(SingleDatabaseManager<MockType> databaseManager)
        {
            return await databaseManager.PerformQuery(conn => Task.FromResult(1));
        }
        private async Task<int> PerformDummyQueryForPostgresExceptionRetry(SingleDatabaseManager<MockType> databaseManager, string database = "ea", string errorCode = "")
        {
            int count = 0;

            return await databaseManager.PerformQuery(conn => {
                count++;
                throw new Npgsql.PostgresException(String.Format("For errorCode: {0}, No of tries: {1}.", errorCode, count.ToString()), "", "", errorCode);
                return Task.FromResult(1);
            }, database);
        }
        [Fact]
        public void CreateManager_NoDatabaseConnectionString()
        {
            // Act / Assert
            Assert.Throws<ArgumentException>(() =>
                new SingleDatabaseManager<MockType>(
                    String.Empty, DefaultMocks.MockAzureServiceTokenProvider().Object)
            );
        }

        [Fact]
        public void CreateManager_MalformedDatabaseConnectionString()
        {
            // Arrange
            var malformedString = "not a connection string";

            // Act / Assert
            Assert.Throws<ArgumentException>(() => new SingleDatabaseManager<MockType>(
                    malformedString, DefaultMocks.MockAzureServiceTokenProvider().Object)
            );
        }

        [Fact]
        public async void CreateManagerAndPerformQuery_DefaultsToCommercialCloud()
        {
            // Arrange
            ClearEnvironmentVariables();
            var azureTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, azureTokenProvider.Object);

            // Act
            await PerformDummyQuery(databaseManager);

            // Assert
            azureTokenProvider.Verify(m =>
                m.GetAccessTokenAsync(SingleDatabaseManager<MockType>.CommercialId, null, default(CancellationToken)), Times.Once);
        }

        [Fact]
        public async void CreateManagerAndPerformQuery_UsesGovtCloudWhenSet()
        {
            // Arrange
            var azureTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, azureTokenProvider.Object);
            try
            {
                Environment.SetEnvironmentVariable(
                    SingleDatabaseManager<MockType>.CloudName,
                    SingleDatabaseManager<MockType>.GovernmentCloud
                );

                // Act
                await PerformDummyQuery(databaseManager);

                // Assert
                azureTokenProvider.Verify(m =>
                    m.GetAccessTokenAsync(SingleDatabaseManager<MockType>.GovermentId, null, default(CancellationToken)), Times.Once);
            }
            finally
            {
                // Tear down
                ClearEnvironmentVariables();
            }
        }

        [Fact]
        public async void PerformQuery_DoesNotFetchToken_WhenNotNeeded()
        {
            // Arrange
            var azureTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionStringWithoutNeedingToken, azureTokenProvider.Object);

            // Act
            await PerformDummyQuery(databaseManager);

            // Assert
            azureTokenProvider.Verify(m =>
                m.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PerformQuery_UsesTokenAsPassword()
        {
            // Arrange
            var expectedPassword = Guid.NewGuid().ToString();
            var azureTokenProvider = DefaultMocks.MockAzureServiceTokenProvider(expectedPassword);
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, azureTokenProvider.Object);
            string usedConnectionString = "";

            // Act
            await databaseManager.PerformQuery(conn =>
            {
                usedConnectionString = conn.ConnectionString;
                return Task.FromResult(1); // just return a value instead of actually performing a query.
            }, "ea");

            // Assert
            Assert.Contains($"Password={expectedPassword}", usedConnectionString);
        }

        [Fact]
        public async Task PerformQuery_UsesNewTokenAsPassword_WhenAvailable()
        {
            // Arrange
            var azureTokenProvider = new Mock<AzureServiceTokenProvider>(() =>
                new AzureServiceTokenProvider(null, "https://tts.test")
            );

            azureTokenProvider
                .SetupSequence(m => m.GetAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("token1")
                .ReturnsAsync("token2");

            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, azureTokenProvider.Object);
            string usedConnectionString = "";

            // Act
            await databaseManager.PerformQuery(conn =>
            {
                usedConnectionString = conn.ConnectionString;
                return Task.FromResult(1); // just return a value instead of actually performing a query.
            });

            // Assert
            Assert.Contains($"Password=token1", usedConnectionString);

            // Act
            await databaseManager.PerformQuery(conn =>
            {
                usedConnectionString = conn.ConnectionString;
                return Task.FromResult(1); // just return a value instead of actually performing a query.
            });

            // Assert
            Assert.Contains($"Password=token2", usedConnectionString);
        }

        [Fact]
        public async Task DisposingManager_CleansUpResources()
        {
            // Arrange
            var azureTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, azureTokenProvider.Object);

            // Act
            await databaseManager.DisposeAsync();

            // Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await PerformDummyQuery(databaseManager)
            );
        }


        [Fact]
        public async Task PerformQuery_ManyTimes_SimultaneouslyWorksFine()
        {
            // Arrange
            ClearEnvironmentVariables();
            var mockTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, mockTokenProvider.Object);
            int numberOfTasksCompleted = 0;
            int numberOfTasksToDo = 2000;
            int numberOfErrors = 0;

            // Act - Kick off a bunch of tasks with all going to the single database to make sure the database manager can handle multiple threads at once
            Task[] tasks = new Task[numberOfTasksToDo];
            for (int i = 0; i < numberOfTasksToDo; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    try
                    {
                        await databaseManager.PerformQuery(async conn =>
                        {
                            // delay 100 milliseconds to ensure it's multithreaded.
                            await Task.Delay(100);
                            return 1;
                        });
                        Interlocked.Increment(ref numberOfTasksCompleted);
                    }
                    catch
                    {
                        Interlocked.Increment(ref numberOfErrors);
                    }
                });
            }
            await Task.WhenAll(tasks);

            // Wait a small additional amount of time in case any thread is finishing their increment, which is done atomically.
            await Task.Delay(100);

            // Assert that all tasks completed, and that there were no errors.
            Assert.Equal(numberOfTasksToDo, numberOfTasksCompleted);
            Assert.Equal(0, numberOfErrors);
        }
        [InlineData("08000")]
        [InlineData("08001")]
        [InlineData("08007")]
        [InlineData("08006")]
        [InlineData("08004")]
        [InlineData("08003")]

        [Theory]
        public async Task PerformQuery_For_ErrorCodesInPolicyForRetry(string errorCode)
        {
            // Arrange
            var mockTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, mockTokenProvider.Object);
            //Speed up the runtime for tests
            databaseManager.NoOfRetries = 1;
            databaseManager.RetryInterval = 1;
            // Act / Assert.   SingleDatabaseManager  throw an error which is part of the Polly retry policy.
            var ex = await Assert.ThrowsAsync<PostgresException>(() => PerformDummyQueryForPostgresExceptionRetry(databaseManager, "eb", errorCode));
            Assert.Contains(errorCode, ex.Message);
            Assert.Contains("No of tries: " + (databaseManager.NoOfRetries+1).ToString() + ".", ex.Message);
        }

        [InlineData("08002")]
        [InlineData("08005")]

        [Theory]
        public async Task PerformQuery_For_ErrorCodesNotInPolicyForRetry(string errorCode)
        {
            // Arrange
            var mockTokenProvider = DefaultMocks.MockAzureServiceTokenProvider();
            var databaseManager = new SingleDatabaseManager<MockType>(
                    ConnectionString, mockTokenProvider.Object);
            //Speed up the runtime for tests
            databaseManager.NoOfRetries = 1;
            databaseManager.RetryInterval = 1;
            // Act / Assert.   SingleDatabaseManager  throw an error which is part of the Polly retry policy.
            var ex = await Assert.ThrowsAsync<PostgresException>(() => PerformDummyQueryForPostgresExceptionRetry(databaseManager, "eb", errorCode));
            Assert.Contains(errorCode, ex.Message);
            Assert.Contains("No of tries: 1.", ex.Message);
        }
    }
}
