using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Participants.Core.Services
{
    public interface IStateService
    {
        Task<IEnumerable<string>> GetStates();
    }
}