using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Participants.Core.Models;

namespace Piipan.Participants.Core.DataAccessObjects 
{
    public interface IParticipantDao
    {
        Task<IEnumerable<ParticipantDbo>> GetParticipants(string state, string ldsHash, Int64 uploadId);
        Task<ulong> AddParticipants(IEnumerable<ParticipantDbo> participants);
        Task DeleteOldParticipantsExcept(string state, long uploadId);
    }
}