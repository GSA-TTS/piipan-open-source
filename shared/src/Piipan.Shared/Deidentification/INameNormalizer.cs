using System;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Public interface for NameNormalizer class.
    /// </summary>
    public interface INameNormalizer
    {
        string Run(string lname);
    }
}
