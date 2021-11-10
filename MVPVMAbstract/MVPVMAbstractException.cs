using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MVPVMAbstract
{
    public class MVPVMAbstractException : Exception
    {
        public MVPVMAbstractException()
        {
        }

        public MVPVMAbstractException(string message) : base(message)
        {
        }

        public MVPVMAbstractException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MVPVMAbstractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
