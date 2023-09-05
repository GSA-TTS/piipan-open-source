using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Piipan.Participants.Api.Models;

namespace Piipan.Participants.Api
{
    public interface IParticipantApi
    {
        Task<IEnumerable<IParticipant>> GetParticipants(string state, string ldsHash);
        Task AddParticipants(IEnumerable<IParticipant> participants, IUpload upload, string state, Func<Exception, string> redacteErrorMessageCallback);
        Task<IEnumerable<string>> GetStates();
        Task DeleteOldParticpants(string state = null);
    }
}