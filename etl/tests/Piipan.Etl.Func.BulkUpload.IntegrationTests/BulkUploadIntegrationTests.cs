using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Dapper;
using Dapper.NodaTime;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Validators;
using Piipan.Metrics.Api;
using Piipan.Participants.Api;
using Piipan.Participants.Core.DataAccessObjects;
using Piipan.Participants.Core.Services;
using Piipan.Shared.API.Utilities;
using Piipan.Shared.Common;
using Piipan.Shared.Cryptography;
using Piipan.Shared.Cryptography.Extensions;
using Piipan.Shared.Database;
using Piipan.Shared.Deidentification;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.IntegrationTests
{
    public class BulkUploadIntegrationTests : DbFixture
    {
        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            string base64EncodedKey = "kW6QuilIQwasK7Maa0tUniCdO+ACHDSx8+NYhwCo7jQ=";
            Environment.SetEnvironmentVariable("ColumnEncryptionKey", base64EncodedKey);

            services.AddLogging();
           
            services.AddSingleton<IDatabaseManager<ParticipantsDbManager>>(s =>
            {
                return new SingleDatabaseManager<ParticipantsDbManager>(ConnectionString);
            });

           
            services.AddTransient<IParticipantStreamParser, ParticipantCsvStreamParser>();
            
            services.AddTransient<IBlobClientStream>(b =>
            {
                var factory = new Mock<IBlobClientStream>();
                factory
                        .Setup(m => m.Parse(
                                        It.IsAny<string>(),     //EventGridEvent string
                                        It.IsAny<ILogger>()))
                        .Returns(() =>
                        {
                            var responseMock = new Mock<Response>();
                            Stream stream = new MemoryStream(File.ReadAllBytes("example.csv"));
                            var blockBlobClient = new Mock<BlockBlobClient>();

                            blockBlobClient
                                .Setup(m => m.GetProperties(null, CancellationToken.None))
                                .Returns(Response.FromValue<BlobProperties>(new BlobProperties(), responseMock.Object));

                            blockBlobClient
                                .Setup(m => m.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), CancellationToken.None))
                                .Returns(new Mock<Response<bool>>().Object);

                            blockBlobClient
                                    .Setup(m => m.DownloadTo(It.IsAny<Stream>()))
                                    .Callback((Stream target) => { stream.CopyTo(target); target.Position = 0; })
                                    .Returns(new Mock<Response>().Object);

                            blockBlobClient
                                    .Setup(m => m.OpenReadAsync(0, null, null, default))
                                    .Returns(Task.FromResult(stream));


                            return blockBlobClient.Object;
                        });


                return factory.Object;
            });

            services.RegisterKeyVaultClientServices();

            // Next lines replace services.RegisterParticipantsServices();
            services.AddTransient<IParticipantBulkInsertHandler, ParticipantBulkInsertHandler>();
            services.AddTransient<IParticipantDao, ParticipantDao>();
            services.AddTransient<IUploadDao, UploadDao>();
            services.AddTransient<IStateService, StateService>();
            services.AddTransient<IParticipantApi, ParticipantService>();
            services.AddTransient<ICsvValidator, CsvValidator>();
            services.AddTransient<IValidator<ParticipantCsv>, ParticipantValidator>();
            services.AddTransient<IParticipantUploadApi, ParticipantUploadService>();

            services.AddTransient<IParticipantPublishUploadMetric>(b =>
            {
                var factory = new Mock<IParticipantPublishUploadMetric>();

                factory.Setup(m => m.PublishUploadMetric(
                                        It.IsAny<ParticipantUpload>()))
                       .Returns(Task.CompletedTask);

                return factory.Object;
            });


            return services.BuildServiceProvider();
        }

        private BulkUpload BuildFunction()
        {
            var services = BuildServices();
            return new BulkUpload(
                services.GetService<IParticipantApi>(),
                services.GetService<IParticipantUploadApi>(),
                services.GetService<IParticipantStreamParser>(),
                services.GetService<IBlobClientStream>(),
                services.GetService<ICsvValidator>(),
                services.GetService<IRedactionService>()
            );
        }

        [Fact]
        public async void SavesCsvRecords()
        {
            // setup
            var services = BuildServices();
            ClearParticipants();

            var function = BuildFunction();

            // act
            await function.Run(
                "Event Grid Event String",
                Mock.Of<ILogger>()
            );

            AzureAesCryptographyClient cryptographyClient = new AzureAesCryptographyClient();
            var records = QueryParticipants("SELECT * from participants;").ToList();

            // assert Need to check for Encryption,  While Decrypt we are getting some padding with \0. Need to check
            for (int i = 0; i < records.Count(); i++)
            {
                if (i <= 37)
                {
                    Assert.Equal($"caseid{i + 1}", cryptographyClient.DecryptFromBase64String(records.ElementAt(i).CaseId));
                }

                Assert.Equal($"participantid{i + 1}", cryptographyClient.DecryptFromBase64String(records.ElementAt(i).ParticipantId));
            }

            string firstParticipantHash = "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec";
            string decryptedHash = cryptographyClient.DecryptFromBase64String(records.First().LdsHash);
            Assert.Equal(firstParticipantHash, decryptedHash);
            Assert.Equal(new DateTime(2021, 05, 15), records.First().ParticipantClosingDate);
            Assert.Equal(new DateRange(new DateTime(2021, 04, 01), new DateTime(2021, 04, 15)), records.First().RecentBenefitIssuanceDates.First());
            Assert.Equal(new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 30)), records.First().RecentBenefitIssuanceDates.ElementAt(1));
            Assert.Equal(new DateRange(new DateTime(2021, 02, 01), new DateTime(2021, 02, 28)), records.First().RecentBenefitIssuanceDates.ElementAt(2));
            Assert.True(records.First().VulnerableIndividual);
        }
    }
}
