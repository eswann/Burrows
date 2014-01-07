using System;
using System.Runtime.Serialization;

namespace Burrows.Tests.Framework
{
    [Serializable]
    public class ATestException: Exception
    {
        public ATestException() : this("This exception was thrown as part of a designed test")
        {
        }

        public ATestException(string message)
            : base(message)
        {
        }

        public ATestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ATestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}