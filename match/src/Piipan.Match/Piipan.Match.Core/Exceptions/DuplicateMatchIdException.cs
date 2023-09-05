using System;
using System.Data.Common;

namespace Piipan.Match.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a MatchEmailDetails fails to insert because of a MatchId collision.
    /// </summary>
    public class DuplicateMatchIdException : DbException
    {
        /// <summary>
        /// Initializes a new instance of the DuplicateMatchIdException class.
        /// </summary>
        public DuplicateMatchIdException() { }

        /// <summary>
        /// Initializes a new instance of the DuplicateMatchIdException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public DuplicateMatchIdException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
