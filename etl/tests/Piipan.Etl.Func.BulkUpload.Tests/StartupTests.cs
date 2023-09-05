using System;
using Piipan.Shared.Tests.DependencyInjection;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests
{
    public class StartupTests
    {
        [Fact]
        public void Configure_AllServicesResolve()
        {
            // Arrange
            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            Environment.SetEnvironmentVariable(Startup.ParticipantsDatabaseConnectionString,
                "Server=server;Database=db;Port=5432;User Id=postgres;Password={password};");

            Environment.SetEnvironmentVariable("EventGridEndPoint", "http://someendpoint.gov");
            Environment.SetEnvironmentVariable("EventGridKeyString", "example");

            var container = new DependencyTester()
                .Register<BulkUpload>()
                .Register<GetUploadStatusApi>();

            // Act/Assert
            container.ValidateFunctionServices<Startup>();
        }
    }
}