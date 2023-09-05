using System.Collections.Generic;

namespace Piipan.QueryTool.Client.Helpers
{
    public static class StateHelper
    {
        private static Dictionary<string, string> stateMaps = new Dictionary<string, string>()
        {
            {"al",  "Alabama"},
            {"ak",  "Alaska"},
            {"az",  "Arizona"},
            {"ar",  "Arkansas"},
            {"ca",  "California"},
            {"cz",  "Canal Zone"},
            {"co",  "Colorado"},
            {"ct",  "Connecticut"},
            {"de",  "Delaware"},
            {"dc",  "District of Columbia"},
            {"fl",  "Florida"},
            {"ga",  "Georgia"},
            {"gu",  "Guam"},
            {"hi",  "Hawaii"},
            {"id",  "Idaho"},
            {"il",  "Illinois"},
            {"in",  "Indiana"},
            {"ia",  "Iowa"},
            {"ks",  "Kansas"},
            {"ky",  "Kentucky"},
            {"la",  "Louisiana"},
            {"me",  "Maine"},
            {"md",  "Maryland"},
            {"ma",  "Massachusetts"},
            {"mi",  "Michigan"},
            {"mn",  "Minnesota"},
            {"ms",  "Mississippi"},
            {"mo",  "Missouri"},
            {"mt",  "Montana"},
            {"ne",  "Nebraska"},
            {"nv",  "Nevada"},
            {"nh",  "New Hampshire"},
            {"nj",  "New Jersey"},
            {"nm",  "New Mexico"},
            {"ny",  "New York"},
            {"nc",  "North Carolina"},
            {"nd",  "North Dakota"},
            {"oh",  "Ohio"},
            {"ok",  "Oklahoma"},
            {"or",  "Oregon"},
            {"pa",  "Pennsylvania"},
            {"pr",  "Puerto Rico"},
            {"ri",  "Rhode Island"},
            {"sc",  "South Carolina"},
            {"sd",  "South Dakota"},
            {"tn",  "Tennessee"},
            {"tx",  "Texas"},
            {"ut",  "Utah"},
            {"vt",  "Vermont"},
            {"vi",  "Virgin Islands"},
            {"va",  "Virginia"},
            {"wa",  "Washington"},
            {"wv",  "West Virginia"},
            {"wi",  "Wisconsin"},
            {"wy",  "Wyoming" },
            // Test States
            {"ea",  "Echo Alpha"},
            {"eb",  "Echo Bravo"},
            {"ec",  "Echo Charlie"},
        };
        public static string GetStateName(string abbreviation)
        {
            abbreviation = abbreviation.ToLower();
            return stateMaps.ContainsKey(abbreviation) ? stateMaps[abbreviation] : "";
        }
    }
}
