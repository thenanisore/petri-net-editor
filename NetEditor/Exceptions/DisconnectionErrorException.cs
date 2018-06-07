using System;

namespace NetEditor.Exceptions
{
    /// <summary>
    /// Thrown in case of an error during the node disconnection.
    /// </summary>
    class DisonnectionErrorException : Exception
    {
        public DisonnectionErrorException()
            : base("Nodes disconnection error.") { }

        public DisonnectionErrorException(string message)
            : base(message) { }
    }
}
