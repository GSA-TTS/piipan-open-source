using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Piipan.Metrics.Api;
using Piipan.Shared.API.Validation;
using Piipan.Shared.Helpers;

#nullable enable

namespace Piipan.Metrics.Func.Api
{
    public class GetParticipantUploads
    {
        private readonly IParticipantUploadReaderApi _participantUploadApi;


        public GetParticipantUploads(IParticipantUploadReaderApi participantUploadApi)
        {
            _participantUploadApi = participantUploadApi;

        }

        [FunctionName("GetParticipantUploads")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);

            try
            {
                var query = req.Query; // this is IQueryCollection
                var json = JsonConvert.SerializeObject(query.ToDictionary(q => q.Key, q => q.Value.ToString()));
                var filter = JsonConvert.DeserializeObject<ParticipantUploadRequestFilter>(json);

                var results = new List<ValidationResult>();
                ValidationContext dataAnnotationContext = new ValidationContext(filter); // Calling the validation for Start Date and End Date.

                if (!Validator.TryValidateObject(filter, dataAnnotationContext, results, true))
                {
                    var errorMessages = new StringBuilder();

                    foreach (var result in results)
                    {
                        string displayNameAttributeValue = ObjectProperty.GetDisplayNameAttribute(filter.GetType(), result.MemberNames.FirstOrDefault());
                        string? errorMessage = result.ErrorMessage.Replace(ValidationConstants.ValidationFieldPlaceholder, displayNameAttributeValue);
                        log.LogError(errorMessage);

                        errorMessages.AppendLine(errorMessage);
                    }

                    return new JsonResult(errorMessages.ToString());
                }

                var response = await _participantUploadApi.GetUploads(filter);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
