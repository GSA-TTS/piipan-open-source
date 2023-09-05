using System.Collections.Generic;
using System.IO;
using Piipan.Participants.Api.Models;

namespace Piipan.Etl.Func.BulkUpload.Parsers
{
    public interface IParticipantStreamParser
    {
        IEnumerable<IParticipant> Parse(Stream input);
        HashSet<string> GetPersonallyIdentifiableInformation(Stream input);
    }
}