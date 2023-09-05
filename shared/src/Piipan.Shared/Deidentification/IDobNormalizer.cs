using System;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Public interface for DobNormalizer class.
    /// </summary>
    public interface IDobNormalizer
    {
        string Run(string dob);
    }
}
