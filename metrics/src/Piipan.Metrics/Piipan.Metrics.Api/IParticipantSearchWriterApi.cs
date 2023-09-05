using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Api
{
    public interface IParticipantSearchWriterApi
    {
        Task<int> AddSearchMetrics(ParticipantSearch newParticipantSearch);
    }
}
