// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Etl.Func.BulkUpload.Parsers;
using Piipan.Etl.Func.BulkUpload.Validators;
using Piipan.Participants.Api;
using Piipan.Participants.Api.Models;
using Piipan.Shared.Deidentification;

namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing basic Extract-Transform-Load of piipan
    /// bulk import CSV files via Storage Containers, Event Grid, and
    /// PostgreSQL.
    /// </summary>
    public class BulkUpload
    {
        private readonly IParticipantApi _participantApi;
        private readonly IParticipantStreamParser _participantParser;
        private readonly IBlobClientStream _blobStream;
        private readonly ICsvValidator _csvValidator;
        private readonly IRedactionService _redactionService;
        private readonly IParticipantUploadApi _uploadApi;

        public BulkUpload(
            IParticipantApi participantApi,
            IParticipantUploadApi uploadApi,
            IParticipantStreamParser participantParser,
            IBlobClientStream blobStream,
            ICsvValidator csvValidator,
            IRedactionService redactionService)
        {
            _participantApi = participantApi;
            _participantParser = participantParser;
            _blobStream = blobStream;
            _csvValidator = csvValidator;
            _redactionService = redactionService;
            _uploadApi = uploadApi;
        }

        /// <summary>
        /// Entry point for the state-specific Azure Function instance
        /// </summary>
        /// <param name="myQueueItem">storage queue item</param>
        /// <param name="log">handle to the function log</param>
        /// <remarks>
        /// The function is expected to be executing as a managed identity that has read access
        /// to the per-state storage account and write access to the per-state database.
        /// </remarks>
        [FunctionName("BulkUpload")]
        public async Task Run(
            [QueueTrigger("upload", Connection = "BlobStorageConnectionString")] string myQueueItem,
            ILogger log)
        {
            log.LogInformation(myQueueItem);
            try
            {
                if (myQueueItem == null || myQueueItem.Length == 0)
                {
                    log.LogError("No input stream was provided");
                }
                else
                {
                    var blockBlobClient = _blobStream.Parse(myQueueItem, log);

                    using Stream input = await blockBlobClient.OpenReadAsync();

                    log.LogInformation($"Input lenght: {input.Length} Position: {input.Position}");

                    var blobProperties = blockBlobClient.GetProperties().Value;

                    if (input != null)
                    {
                        //Azure documents that ETags are quoted. So we need to remove the quotes in order to get the upload_id
                        var upload_id = blobProperties.ETag.ToString().Replace("\"", "");
                        string state = Environment.GetEnvironmentVariable("State");
                        DateTime startUploadTime = DateTime.UtcNow;

                        IUpload upload = new UploadDto() { UploadIdentifier=upload_id};
                        try
                        {
                            // Large participant uploads can be long-running processes and require
                            // an increased time out duration to avoid System.TimeoutException
                            upload = await _uploadApi.AddUpload(upload_id, state);

                            var validationReport = _csvValidator.ValidateCsvDoc(input, blockBlobClient.Name, upload_id);

                            if (!validationReport.IsValid)
                            {
                                var validationErrorMessage = JsonConvert.SerializeObject(validationReport);

                                await _uploadApi.UpdateUpload(upload, state, validationErrorMessage)
                                            .ContinueWith(t => _blobStream.DeleteBlobAfterProcessing(t, blockBlobClient, log));

                                log.LogError($"Error uploading participants: {validationErrorMessage}");
                                return;
                            }
                        
                        
                            var participants = _participantParser.Parse(input);

                            await _participantApi.AddParticipants(participants, upload, state, (ex) => LogAndRedactErrors(ex))
                                    .ContinueWith(t => _blobStream.DeleteBlobAfterProcessing(t, blockBlobClient, log))
                                    .ContinueWith(t => _participantApi.DeleteOldParticpants());

                            string LogAndRedactErrors(Exception ex)
                            {
                                // reset the participants and input stream. If you only reset the input stream you start with the header row, 
                                // and if you don't reset it you're missing participants that have already been read
                                input.Seek(0, SeekOrigin.Begin);
                                var pii = _participantParser.GetPersonallyIdentifiableInformation(input);

                                var errorDetails = new ParticipantUploadErrorDetails(state, startUploadTime, DateTime.UtcNow, ex, blockBlobClient.Name);

                                var redactedErrorMessage = _redactionService.RedactParticipantsUploadError(errorDetails.ToString(), pii);

                                log.LogError($"Error uploading participants: {redactedErrorMessage}");
                                return redactedErrorMessage;
                            }

                        }
                        catch (Exception e)  //exception catch-all to ensure Upload Record gets set to Failed
                        {
                            await _uploadApi.UpdateUpload(upload, state, "Could not process upload file.")
                                        .ContinueWith(t => _blobStream.DeleteBlobAfterProcessing(t, blockBlobClient, log));
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
