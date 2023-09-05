using System;

namespace Piipan.Shared.Tests.Exceptions
{
    /// <summary>
    /// This exception isn't really an exception, it's just a way to exit the code early, similar to Assert.Pass from NUnit.
    /// </summary>
    public class TestSuccessException : Exception
    {

    }
}