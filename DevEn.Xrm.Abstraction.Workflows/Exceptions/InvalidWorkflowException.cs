using System;

namespace DevEn.Xrm.Abstraction.Workflows.Exceptions
{

    /// <summary>
    /// Exception indicating a failure during workflow activity execution within this abstraction layer.
    /// </summary>
    public class InvalidWorkflowException
        : Exception
    {
        /// <summary>
        /// Creates a new instance with a message describing the error.
        /// </summary>
        public InvalidWorkflowException(string message)
            : base(message) { }

        /// <summary>
        /// Creates a new instance without a message.
        /// </summary>
        public InvalidWorkflowException()
            : base()
        {
        }

        /// <summary>
        /// Creates a new instance with a message and an inner exception detailing the original cause.
        /// </summary>
        public InvalidWorkflowException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
