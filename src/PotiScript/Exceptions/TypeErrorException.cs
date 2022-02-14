using System;
using System.Runtime.Serialization;

namespace PotiScript.Exceptions
{
    [Serializable]
    internal class TypeErrorException : Exception
    {
        public TypeErrorException()
        {
        }

        public TypeErrorException(string? message) : base(message)
        {
        }

        public TypeErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TypeErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}