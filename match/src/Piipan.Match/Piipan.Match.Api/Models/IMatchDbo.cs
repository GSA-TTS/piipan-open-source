using System;

namespace Piipan.Match.Api.Models
{
    public interface IMatchDbo
    {
        string MatchId { get; set; }
        DateTime? CreatedAt { get; set; }
        string Initiator { get; set; }
        string[] States { get; set; }
        string Hash { get; set; }
        string HashType { get; set; }
        string Input { get; set; }
        string Data { get; set; }
    }
}
