using System;
using System.Runtime.Serialization;

namespace ProgramableNetwork.Python
{
    [Serializable]
    public class ReturnException : Exception
    {
        public ReturnException(object value)
        {
            Value = value;
        }

        public ReturnException(string message) : base(message)
        {
        }

        public ReturnException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ReturnException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public object Value { get; }
    }
}