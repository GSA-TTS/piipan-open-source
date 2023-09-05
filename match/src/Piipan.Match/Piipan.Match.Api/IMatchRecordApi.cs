using System.Collections.Generic;
using System.Threading.Tasks;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Api
{
    public interface IMatchRecordApi
    {
        Task<string> AddNewMatchRecord(IMatchDbo record);
        Task<IEnumerable<IMatchDbo>> GetRecords(IMatchDbo record);
    }
}
