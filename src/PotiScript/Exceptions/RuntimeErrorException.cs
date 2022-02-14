using System;
using System.Runtime.Serialization;

namespace PotiScript.Exceptions
{
    [Serializable]
    internal class RuntimeErrorException : Exception
    {
        public RuntimeErrorException()
        {
        }

        public RuntimeErrorException(string? message) : base(message)
        {
        }

        public RuntimeErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RuntimeErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}