using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Piipan.Match.Api.Models;

namespace Piipan.Match.Core.Models
{
    /// <summary>
    /// Implementation of IMatchDbo for database interactions
    /// </summary>
    public class MatchDbo : IMatchDbo
    {
        public string MatchId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Initiator { get; set; }
        public string[] States { get; set; }
        public string Hash { get; set; }
        public string HashType { get; set; }
        [Column(TypeName = "jsonb")]
        public string Input { get; set; }
        [Column(TypeName = "jsonb")]
        public string Data { get; set; }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            MatchDbo m = obj as MatchDbo;
            if (m == null)
            {
                return false;
            }

            return
                MatchId == m.MatchId &&
                Initiator == m.Initiator &&
                States.SequenceEqual(m.States) &&
                Hash == m.Hash &&
                HashType == m.HashType;
        }

        public override int GetHashCode()
        {
            string[] sortedStates = (string[])States.Clone();
            Array.Sort(sortedStates);

            return HashCode.Combine(
                MatchId,
                Initiator,
                String.Join(",", sortedStates),
                Hash,
                HashType
            );
        }
    }
}
