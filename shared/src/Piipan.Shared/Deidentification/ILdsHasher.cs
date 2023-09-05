using System;

namespace Piipan.Shared.Deidentification
{

    /// <summary>
    /// Public interface for LdsHasher class.
    /// </summary>
    public interface ILdsHasher
    {
        public string Run(string lname, string dob, string ssn);
    }
}
