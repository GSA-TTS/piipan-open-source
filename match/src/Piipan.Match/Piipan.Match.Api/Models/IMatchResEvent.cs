using System;

namespace Piipan.Match.Api.Models
{
    /// <summary>
    /// Represents a Match Resolution Event related to a Match Record through its Match ID
    /// </summary>
    public interface IMatchResEvent
    {
        int Id { get; set; }
        string MatchId { get; set; }
        DateTime InsertedAt { get; set; }
        string Actor { get; set; }
        string ActorState { get; set; }
        string Delta { get; set; }
        DateTime? NotifiedAt { get; set; }
    }
}
