using System;
using System.IO;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Match.Api.Models;
using Piipan.Shared.Parsers;

namespace Piipan.Match.Core.Parsers
{
    /// <summary>
    /// Parser for deserializing AddEventRequest objects from a Stream
    /// </summary>
    public class AddEventRequestParser : IStreamParser<AddEventRequest>
    {
        private readonly IValidator<AddEventRequest> _validator;
        private readonly ILogger<AddEventRequestParser> _logger;

        /// <summary>
        /// Initializes a new instance of AddEventRequestParser
        /// </summary>
        public AddEventRequestParser(
            IValidator<AddEventRequest> validator,
            ILogger<AddEventRequestParser> logger)
        {
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Deserializes and validates an AddEventRequest from a Stream
        /// </summary>
        /// <remarks>
        /// Throws ValidationException if FluentValidation fails; throws StreamParserException for all other failures
        /// </remarks>
        /// <param name="stream">A Stream from which a serialized AddEventRequest can be read</param>
        /// <returns>A validated instance of AddEventRequest</returns>
        public async Task<(AddEventRequest ParsedRequest, JObject RawRequest)> Parse(Stream stream)
        {
            try
            {
                AddEventRequest request = null;

                var reader = new StreamReader(stream);
                var serialized = await reader.ReadToEndAsync();

                request = JsonConvert.DeserializeObject<AddEventRequest>(serialized);

                if (request is null)
                {
                    throw new JsonSerializationException("stream must not be empty.");
                }

                var validationResult = await _validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogError($"Found validation errors while parsing: {validationResult.Errors.ToString()}");
                    throw new ValidationException("request validation failed", validationResult.Errors);
                }

                return (request, JObject.Parse(serialized));
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new StreamParserException(ex.Message, ex);
            }
        }
    }
}
