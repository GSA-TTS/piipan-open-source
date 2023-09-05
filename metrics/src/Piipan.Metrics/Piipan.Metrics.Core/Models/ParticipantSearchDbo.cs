using Piipan.Metrics.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Metrics.Core.Models
{
    /// <summary>
    /// Implementation of ParticipantSearch record for Metrics database interactions
    /// </summary>
    public class ParticipantSearchDbo
    {
        public string State { get; set; }
        public string SearchReason { get; set; }
        public string SearchFrom { get; set; }
        public string MatchCreation { get; set; }
        public int MatchCount { get; set; }
        public DateTime SearchedAt { get; set; }
        public ParticipantSearchDbo()
        {
        }

        public ParticipantSearchDbo(ParticipantSearch upload)
        {
            State = upload.State;
            SearchReason = upload.SearchReason;
            SearchFrom = upload.SearchFrom;
            MatchCreation = upload.MatchCreation;
            MatchCount = upload.MatchCount;
            SearchedAt = upload.SearchedAt;
        }
    }
}
