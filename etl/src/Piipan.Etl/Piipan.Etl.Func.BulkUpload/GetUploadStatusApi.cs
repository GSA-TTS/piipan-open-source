using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Piipan.Participants.Api;
using Piipan.Etl.Func.BulkUpload.Models;
using System.ComponentModel.DataAnnotations;

namespace Piipan.Etl.Func.BulkUpload
{
    /// <summary>
    /// Azure Function implementing Get UploadById endpoint for Bulk Upload API
    /// </summary>
    public class GetUploadStatusApi : BaseApi
    {
        private readonly IParticipantUploadApi _uploadService;

        public GetUploadStatusApi(
            IParticipantUploadApi uploadService)
        {
            _uploadService = uploadService;
        }

        [FunctionName("GetUploadStatusById")]
        public async Task<IActionResult> GetUploadById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "uploads/{uploadIdentifier}")] HttpRequest req,
            string uploadIdentifier,
            ILogger logger)
        {
            LogRequest(logger, req);
            try
            {
                if (uploadIdentifier.Length > 40) 
                {
                    throw new ValidationException("Id cannot exceed 40 characters.");
                }
                if (uploadIdentifier.Contains(" "))
                {
                    throw new ValidationException("Id cannot contain a space.");
                }
                    var upload = await _uploadService.GetUploadById(uploadIdentifier);
                
                var response = new UploadStatusApiResponse() { Data = upload };
                return new JsonResult(response) { StatusCode = StatusCodes.Status200OK };
            }
            catch (ValidationException ex)
            {
                logger.LogError(ex, ex.Message);
                return ValidationErrorResponse(ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, ex.Message);
                return NotFoundErrorResponse(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return InternalServerErrorResponse(ex);
            }
        }
    }
}
