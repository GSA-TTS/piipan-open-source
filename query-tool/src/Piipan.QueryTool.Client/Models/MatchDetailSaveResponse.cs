using System.Collections.Generic;
using Piipan.Match.Api.Models.Resolution;

namespace Piipan.QueryTool.Client.Models
{
    /// <summary>
    /// This enum represents the page that we came to Match Detail from. 
    /// We can use it to figure out what the back button should look like and which navigation link to underline.
    /// </summary>
    public enum MatchDetailReferralPage
    {
        Self,
        MatchSearch,
        Query,
        List,
        Other
    }

    /// <summary>
    /// The response returned after an update to match details. We get back a response containing the saved match and any alerts
    /// that we need to display that occurred during the save process.
    /// </summary>
    public class MatchDetailSaveResponse
    {
        /// <summary>
        /// The match after saving
        /// </summary>
        public MatchResApiResponse SavedMatch { get; set; }

        /// <summary>
        /// Any alerts that we need to display after saving the match details
        /// </summary>
        public List<Alert> Alerts { get; set; } = new();
    }
}
