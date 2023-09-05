using System;

namespace Piipan.Shared.Deidentification
{
    /// <summary>
    /// Public interface for LdsDeidentifier class.
    /// </summary>
    public interface ILdsDeidentifier
    {
        string Run(string lname, string dob, string ssn);
    }
}
