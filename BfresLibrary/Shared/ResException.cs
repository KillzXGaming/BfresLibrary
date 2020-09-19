using System;

namespace BfresLibrary
{
    /// <summary>
    /// Represents an exception raised when handling <see cref="ResFile"/> data.
    /// </summary>
    public class ResException : Exception
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ResException"/> class with a specified error
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ResException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResException"/> class with a specified error message created
        /// from the given <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">The format of the error message.</param>
        /// <param name="args">The parameters to format the error message with.</param>
        public ResException(string format, params object[] args)
            : base(String.Format(format, args))
        {
        }
    }
}
