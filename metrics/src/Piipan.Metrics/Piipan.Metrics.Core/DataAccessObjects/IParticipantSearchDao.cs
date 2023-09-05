using Piipan.Metrics.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Core.DataAccessObjects
{
    public interface IParticipantSearchDao
    {
        Task<int> AddParticipantSearchRecord(ParticipantSearchDbo newSearchDbo);
    }
}
