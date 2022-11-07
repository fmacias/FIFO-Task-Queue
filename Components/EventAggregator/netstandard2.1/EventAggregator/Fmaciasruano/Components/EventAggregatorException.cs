using System;
using System.Runtime.Serialization;

namespace EventAggregator.Fmaciasruano.Components
{
    public class EventAggregatorException : Exception
    {
        public EventAggregatorException()
        {
        }

        public EventAggregatorException(string message) : base(message)
        {
        }

        public EventAggregatorException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventAggregatorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
