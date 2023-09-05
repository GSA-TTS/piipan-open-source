namespace Piipan.Match.Core.Services
{
    /// <summary>
    /// Service for generating match IDs
    /// </summary>
    public class MatchIdService : IMatchIdService
    {
        private const string Chars = "23456789BCDFGHJKLMNPQRSTVWXYZ";
        private const int Length = 7;

        /// <summary>
        /// Generate a new ID
        /// </summary>
        /// <remarks>
        /// IDs are generated from a large pool but are not guaranteed to be unique between calls.
        /// </remarks>
        /// <returns>Seven character long string using only the characters found in "23456789BCDFGHJKLMNPQRSTVWXYZ"</returns>
        public string GenerateId()
        {
            return Nanoid.Nanoid.Generate(Chars, Length);
        }
    }
}
