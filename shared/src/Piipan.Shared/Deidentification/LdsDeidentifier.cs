using System;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Tool for deidentifying Personally Identifiable Information (PII)
    /// in the acceptable format for the Bulk Upload and Matching systems
    /// as described in PPRL approach (/docs/pprl.md).
    /// </summary>
    public class LdsDeidentifier : ILdsDeidentifier
    {
        private readonly INameNormalizer _nameNormalizer;
        private readonly IDobNormalizer _dobNormalizer;
        private readonly ISsnNormalizer _ssnNormalizer;
        private readonly ILdsHasher _ldsHasher;

        public LdsDeidentifier(
            INameNormalizer nameNormalizer,
            IDobNormalizer dobNormalizer,
            ISsnNormalizer ssnNormalizer,
            ILdsHasher ldsHasher
        ){
            _nameNormalizer = nameNormalizer;
            _dobNormalizer = dobNormalizer;
            _ssnNormalizer = ssnNormalizer;
            _ldsHasher = ldsHasher;
        }

        /// <summary>
        /// Runs the complete deidentification process.
        /// </summary>
        /// <param name="lname">last name of individual</param>
        /// <param name="dob">date of birth of individual</param>
        /// <param name="ssn">social security number of individual</param>
        public string Run(string lname, string dob, string ssn)
        {
            return _ldsHasher.Run(
                _nameNormalizer.Run(lname),
                _dobNormalizer.Run(dob),
                _ssnNormalizer.Run(ssn)
            );
        }
    }
}
