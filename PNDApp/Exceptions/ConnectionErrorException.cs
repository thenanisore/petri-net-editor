using System;

namespace PNDApp.Exceptions
{
    /// <summary>
    /// Thrown in case of an error during the node connection.
    /// </summary>
    class ConnectionErrorException : Exception
    {
        public ConnectionErrorException() 
            : base("Nodes connection error.") { }

        public ConnectionErrorException(string message) 
            : base(message) { }
    }
}
