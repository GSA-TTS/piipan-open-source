using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

#nullable enable

namespace Piipan.Shared.Parsers
{
    public class StreamParserException : Exception
    {
        public StreamParserException(string? message)
            : base(message)
        {
        }

        public StreamParserException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }

    public interface IStreamParser<T>
    {
        Task<(T ParsedRequest, JObject RawRequest)> Parse(Stream stream);
    }
}