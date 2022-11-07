using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IQueue
    {
        Task<IQueue> Complete();
        Task<IQueue> CancelAfter(int taskCancellationTime);
        void CancelExecution();
        TaskScheduler TaskScheduler { get; }
        CancellationToken CancellationToken { get; }
        List<Task> Tasks { get; }
    }
}
