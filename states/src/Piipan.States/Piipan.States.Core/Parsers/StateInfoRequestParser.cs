using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Piipan.Shared.Parsers;
using Piipan.States.Api.Models;

namespace Piipan.States.Core.Parsers
{
    /// <summary>
    /// Parser for deserializing Upsert_state_info request objects from a Stream
    /// </summary>
    public class StateInfoRequestParser : IStreamParser<StateInfoRequest>
    {
        private readonly ILogger<StateInfoRequestParser> _logger;

        /// <summary>
        /// Initializes a new instance of StateInfoRequestParser
        /// </summary>
        public StateInfoRequestParser(ILogger<StateInfoRequestParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Deserializes and validates an Upsert State info request from a Stream
        /// </summary>
        /// <remarks>
        /// Throws ValidationException if FluentValidation fails; throws StreamParserException for all other failures
        /// </remarks>
        /// <param name="stream">A Stream from which a serialized AddEventRequest can be read</param>
        public async Task<(StateInfoRequest ParsedRequest, JObject RawRequest)> Parse(Stream stream)
        {
            try
            {
                StateInfoRequest request = null;

                var reader = new StreamReader(stream);
                var serialized = await reader.ReadToEndAsync();

                request = JsonConvert.DeserializeObject<StateInfoRequest>(serialized);

                if (request is null)
                {
                    throw new JsonSerializationException("stream must not be empty.");
                }

                return (request, JObject.Parse(serialized));
            }
            catch (Exception ex)
            {
                throw new StreamParserException(ex.Message, ex);
            }
        }
    }
}
