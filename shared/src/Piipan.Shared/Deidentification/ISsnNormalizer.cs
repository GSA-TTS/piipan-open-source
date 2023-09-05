using System;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Public interface for SsnNormalizer class.
    /// </summary>
    public interface ISsnNormalizer
    {
        public string Run(string ssn);
    }
}
