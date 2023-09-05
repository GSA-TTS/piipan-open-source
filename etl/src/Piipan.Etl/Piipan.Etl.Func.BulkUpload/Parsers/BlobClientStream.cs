using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid.SystemEvents;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public class BlobClientStream : IBlobClientStream
    {
        private const string CUSTOMER_KEY_FUNC_VARIABLE_NAME = "UploadPayloadKey";

        public virtual string GetBlobName(StorageBlobCreatedEventData blobEvent) {

                //Get blob name from the blob url
                var blobUrl = new Uri(blobEvent.Url);
                BlobUriBuilder blobUriBuilder = new BlobUriBuilder(blobUrl);

                var blobName = blobUriBuilder.BlobName;

                return blobName;
        }

        public virtual BlockBlobClient GetBlob(string blobName, string connectionString = "BlobStorageConnectionString") {
            string uploadEncryptionKey = Environment.GetEnvironmentVariable(CUSTOMER_KEY_FUNC_VARIABLE_NAME);
            string storageConnectionString = Environment.GetEnvironmentVariable(connectionString);
            BlobClientOptions blobClientOptions = new BlobClientOptions() { CustomerProvidedKey = new CustomerProvidedKey(uploadEncryptionKey) };
            return new BlockBlobClient(storageConnectionString, "upload", blobName, blobClientOptions);
        }

        private StorageBlobCreatedEventData ParseEvents(string input){

            //parse queue event
            var queuedEvent = Azure.Messaging.EventGrid.EventGridEvent.Parse(BinaryData.FromString(input));

            return queuedEvent.Data.ToObjectFromJson<StorageBlobCreatedEventData>();
        }

        public BlockBlobClient Parse(string input, ILogger log) {
            try
            {
                var createdBlobEvent = ParseEvents(input);

                var blobName = GetBlobName(createdBlobEvent);

                BlockBlobClient blob = GetBlob(blobName);

                return blob;
            }
            catch (System.Exception ex) {
                throw ex;
            }
        }
        public Azure.Response<bool> DeleteBlobAfterProcessing(Task antecedent, BlockBlobClient blockBlobClient, ILogger log){
                
                if (antecedent.Status == TaskStatus.RanToCompletion)
                {
                    return blockBlobClient.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                }
                else 
                {
                    log.LogError("Error inserting participants, blob not deleted.");
                    return blockBlobClient.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                }             
        }
    }
}
