using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Piipan.Etl.Func.BulkUpload.Models;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Shared.API.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Etl.BulkUpload.Performance.TestRunner
{
    public class BulkUploadPerfTestRunner
    {
        private const string UPLOAD_CONTAINER_NAME = "upload";
        private string _azureStorageAccountName;
        private string _azureStorageAccountKey;
        private string _uploadEncryptionKey;

        public BulkUploadPerfTestRunner(string azureStorageAccountName, string azureStorageAccountKey, string uploadEncryptionKey)
        {
            _azureStorageAccountName = azureStorageAccountName;
            _azureStorageAccountKey = azureStorageAccountKey;
            _uploadEncryptionKey = uploadEncryptionKey;
        }

        public async Task runTest(long desiredParticipantCount)
        {
            string headers = "lds_hash,case_id,participant_id,participant_closing_date,recent_benefit_issuance_dates,vulnerable_individual";

            using (MemoryStream ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    Console.WriteLine($"Begin populating Mock Records - {DateTime.Now.ToLongTimeString()}");
                    await PopulateMemoryStreamWithMockRecords(writer, desiredParticipantCount);
                    Console.WriteLine($"Finish populating Mock Records - {DateTime.Now.ToLongTimeString()}");

                    var cpk = new CustomerProvidedKey(_uploadEncryptionKey);

                    // Specify the customer-provided key on the options for the client.
                    BlobClientOptions options = new BlobClientOptions()
                    {
                        CustomerProvidedKey = cpk
                    };

                    string connectionString = $"DefaultEndpointsProtocol=https;AccountName={_azureStorageAccountName};AccountKey={_azureStorageAccountKey};EndpointSuffix=core.usgovcloudapi.net";
                    var blobClient = new BlobContainerClient(connectionString, UPLOAD_CONTAINER_NAME, options);

                    string datePostfix = DateTime.Now.ToString("MM-dd-yy_HH:mm:ss");

                    var blob = blobClient.GetBlobClient($"perfTestUpload-{datePostfix}.csv");

                    ms.Seek(0, SeekOrigin.Begin);

                    Console.WriteLine($"Begin upload to Azure Storage - {DateTime.Now.ToLongTimeString()}");
                    Task uploadResult = blob.UploadAsync(ms);
                    uploadResult.Wait();
                    Console.WriteLine($"Finish upload to Azure Storage - {DateTime.Now.ToLongTimeString()}");
                }
            }
        }


        private static async Task PopulateMemoryStreamWithMockRecords(StreamWriter writer, long desiredParticipantCount)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            //StringWriter stringWriter = new StringWriter();
            var csvwriter = new CsvWriter(writer, config);
            csvwriter.Context.RegisterClassMap<ParticipantMap>();

            csvwriter.WriteHeader<Participant>();
            csvwriter.NextRecord();

            var state = "EA";

            for (int i = 0; i < desiredParticipantCount; i++)
            {
                var p = new Participant();

                var recId = i + 1;
                var padRecId = recId.ToString("00000000");
                p.LdsHash = createMockHash(128);

                p.CaseId = $"case{state}{padRecId}";
                p.ParticipantId = $"part{state}{padRecId}";
                p.ParticipantClosingDate = DateTime.Now;

                var dr1 = new DateRange(new DateTime(2021, 04, 01), new DateTime(2021, 04, 15));
                var dr2 = new DateRange(new DateTime(2021, 03, 01), new DateTime(2021, 03, 30));
                var dr3 = new DateRange(new DateTime(2021, 02, 01), new DateTime(2021, 02, 28));
                p.RecentBenefitIssuanceDates = new List<DateRange>() { dr1, dr2, dr3 };
                p.VulnerableIndividual = true;

                csvwriter.WriteRecord(p);
                csvwriter.NextRecord();
            }

            csvwriter.Flush();

        }

        private static String createMockHash(int len)
        {
            StringBuilder mockHash = new StringBuilder(128);

            var random = RandomNumberGenerator.Create(); 
            
            for (int i = 0; i < 16; i++)
            {
                byte[] data = new byte[16];
                random.GetBytes(data);
                int randomNumber = Math.Abs(BitConverter.ToInt32(data));
                int digit = (int)Math.Floor((decimal)randomNumber);
                string hexStringInLowercase = digit.ToString("X8").ToLower();
                mockHash.Append(hexStringInLowercase);

            }

            return mockHash.ToString();

        }
    }
}
