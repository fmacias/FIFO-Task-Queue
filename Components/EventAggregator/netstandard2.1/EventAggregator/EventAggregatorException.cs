using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace fmacias.Components.EventAggregator
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
