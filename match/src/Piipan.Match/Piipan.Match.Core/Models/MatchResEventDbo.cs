using System;
using System.ComponentModel.DataAnnotations.Schema;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Implementation of IMatchResEvent for database interactions
    /// </summary>
    public class MatchResEventDbo : IMatchResEvent
    {
        public int Id { get; set; }
        public string MatchId { get; set; }
        public DateTime InsertedAt { get; set; }
        public string Actor { get; set; }
        [Column(TypeName = "jsonb")]
        public string Delta { get; set; }
        public string ActorState { get; set; }
        public DateTime? NotifiedAt { get; set; }
    }
}
