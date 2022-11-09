using System;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class FifoTaskQueueWorkflowException:Exception
    {
        public FifoTaskQueueWorkflowException()
        {
        }

        public FifoTaskQueueWorkflowException(string message)
            : base(message)
        {
        }

        public FifoTaskQueueWorkflowException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
