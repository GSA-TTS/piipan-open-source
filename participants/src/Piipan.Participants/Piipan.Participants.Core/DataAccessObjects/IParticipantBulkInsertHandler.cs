using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects
{
    public interface IParticipantBulkInsertHandler
    {
        Task<ulong> LoadParticipants(IEnumerable<ParticipantDbo> participants, IDbConnection dbConnection, string tableName);
    }
}
