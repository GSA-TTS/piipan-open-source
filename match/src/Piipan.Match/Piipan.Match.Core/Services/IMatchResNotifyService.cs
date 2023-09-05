using System.Threading.Tasks;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.Match.Core.Services
{
    public interface IMatchResNotifyService
    {
        Task SendNotification(MatchDetailsDto matchResRecordBeforeUpdate, MatchDetailsDto matchResRecordAfterUpdate);
    }
}
