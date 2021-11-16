using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    public class CurrentContextFifoTaskQueue : FifoTaskQueue, ICurrentContextFifoTaskQueue
    {
        public CurrentContextFifoTaskQueue(ILogger logger) : base(TaskShedulerWraper.Create().FromCurrentWorker(), logger)
        {
        }
    }
}
