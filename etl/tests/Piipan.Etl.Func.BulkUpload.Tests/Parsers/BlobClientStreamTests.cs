using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Moq;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Xunit;

namespace Piipan.Etl.Func.BulkUpload.Tests.Parsers
{
    public class BlobClientStreamTests
    {

        private string EventString = "{\"topic\":\"/subscriptions/719bb99b-1a3b-4132-a0f6-1805a75dc30e/resourceGroups/rg-core-dev/providers/Microsoft.Storage/storageAccounts/ttssteauploaddev\",\"subject\":\"/blobServices/default/containers/upload/blobs/example333.csv\",\"eventType\":\"Microsoft.Storage.BlobCreated\",\"id\":\"0b6dcc46-401e-00eb-2f8b-5946db06a8ee\",\"data\":{\"api\":\"PutBlob\",\"requestId\":\"0b6dcc46-401e-00eb-2f8b-5946db000000\",\"eTag\":\"0x8DA27A31DCB5337\",\"contentType\":\"text/plain\",\"contentLength\":6592,\"blobType\":\"BlockBlob\",\"url\":\"https://ttssteauploaddev.blob.core.windows.net/upload/example333.csv\",\"sequencer\":\"00000000000000000000000000002A0D0000000001b0fc63\",\"storageDiagnostics\":{\"batchId\":\"ce49b6a6-f006-00f8-008b-598bff000000\"}},\"dataVersion\":\"\",\"metadataVersion\":\"1\",\"eventTime\":\"2022-04-26T16:37:55.9373378Z\"}";
        private string CUSTOMER_KEY_FUNC_VARIABLE_NAME = "UploadPayloadKey";
        private string TEST_KEY = "testEncryptionKey";

        public BlobClientStreamTests()
        {
            byte[] bytesForKey = Encoding.UTF8.GetBytes(TEST_KEY);
            var base64Key = Convert.ToBase64String(bytesForKey);
            Environment.SetEnvironmentVariable(CUSTOMER_KEY_FUNC_VARIABLE_NAME, base64Key);
        }

        private void VerifyLogError(Mock<ILogger> logger, String expected)
        {
            logger.Verify(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == expected),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
            ));
        }

        [Fact]
        public async void Parse_EmptyEvent()
        {
            //Arrange
            var logger = new Mock<ILogger>();

            var blobClientStream = new BlobClientStream();

            // Act 
            Action act = () => blobClientStream.Parse("", logger.Object);
            
            // Assert
            Assert.ThrowsAny<JsonException>(act);
        }

        [Fact]
        public async void Parse_Event()
        {
            //Arrange
            var logger = new Mock<ILogger>();

            BlockBlobClient blobClient = new BlockBlobClient(new Uri("http://www.contoso.com/blob"), null);
            
            var blobClientStream = new Mock<BlobClientStream>();
                blobClientStream
                        .Setup(x=>x.GetBlob(It.IsAny<String>(), It.IsAny<String>()))
                        .Returns(blobClient);

            // Act 
            BlockBlobClient blobResult = blobClientStream.Object.Parse(EventString, logger.Object);
            
            // Assert
            Assert.Equal(typeof(BlockBlobClient), blobResult.GetType());
        }

        [Fact]
        public void GetBlobName_TestReturnedName()
        {

            // Arrange
            var queuedEvent = Azure.Messaging.EventGrid.EventGridEvent.Parse(BinaryData.FromString(EventString));
            var createdBlobEvent = queuedEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();
            var blobClientStream = new BlobClientStream();

            // Act
            var blobName = blobClientStream.GetBlobName(createdBlobEvent);

            //Assert
            Assert.Equal("example333.csv", blobName);

        }

        [Fact]
        public void GetBlob_ThrowErrorConnectionString()
        {

            // Arrange
            var blobClientStream = new BlobClientStream();

            // Act
            Action act = () => {var blob = blobClientStream.GetBlob("test", "wrongConnectionString");};
            
            // Assert
            var ex = Assert.ThrowsAny<ArgumentNullException>(act);
            Assert.Equal("Value cannot be null.", ex.Message.ToString().Substring(0, 21));

        }

        [Fact]
        public void GetBlob_TestBlob()
        {

            // Arrange
            var blobClientStream = new BlobClientStream();
            Environment.SetEnvironmentVariable("BlobStorageConnectionString", "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=test;AccountKey=abcdefghijklmnopTot4sLIYzB8qKbs9z5Ync5lVy0hOtnr4UFf/mjGFTHhT+Ef0hfGr5kmqjHCq+AStap+0ZA==");

            // Act
            var blob = blobClientStream.GetBlob("test");
            
            // Assert
            Assert.Equal(typeof(BlockBlobClient), blob.GetType());

        }

        [Fact]
        public void DeleteBlobAfterProcessing_TestDeleteBlobTrue()
        {

            // Arrange
            var logger = new Mock<ILogger>();
            var blobClientStream = new BlobClientStream();
            var responseMock = new Mock<Response>();

            var blob = new Mock<BlockBlobClient>();

                blob
                    .Setup(x=>x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), CancellationToken.None))
                    .Returns(Response.FromValue<bool>(true, responseMock.Object));
            
            // Act
            Task t = Task.Run(() => {return true;});
            var response = blobClientStream.DeleteBlobAfterProcessing(t, blob.Object, logger.Object);
            
            // Assert
            Assert.Equal(true, response);

        }
    
        [Fact]
        public void DeleteBlobAfterProcessing_TestDeleteBlobFaultedTask()
        {

            // Arrange
            var logger = new Mock<ILogger>();
            var blobClientStream = new BlobClientStream();
            var responseMock = new Mock<Response>();

            var blob = new Mock<BlockBlobClient>();

                blob
                    .Setup(x=>x.DeleteIfExists(It.IsAny<DeleteSnapshotsOption>(), It.IsAny<BlobRequestConditions>(), CancellationToken.None))
                    .Returns(Response.FromValue<bool>(false, responseMock.Object));
                    
            
            // Act
            Task t = Task.FromException<System.Exception>(new Exception());
            var response = blobClientStream.DeleteBlobAfterProcessing(t, blob.Object, logger.Object);
            
            // Assert
            Assert.Equal(false, response);
            VerifyLogError(logger, "Error inserting participants, blob not deleted.");

        }
    
    }
}
