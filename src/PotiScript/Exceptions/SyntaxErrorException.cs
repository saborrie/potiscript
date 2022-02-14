using System;
using System.Runtime.Serialization;

namespace PotiScript.Exceptions
{
    [Serializable]
    public class SyntaxErrorException : Exception
    {
        public SyntaxErrorException()
        {
        }

        public SyntaxErrorException(string? message) : base(message)
        {
        }

        public SyntaxErrorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SyntaxErrorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
