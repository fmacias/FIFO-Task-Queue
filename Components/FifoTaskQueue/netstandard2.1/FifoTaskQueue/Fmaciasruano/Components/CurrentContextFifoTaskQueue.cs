using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class CurrentContextFifoTaskQueue : FifoTaskQueue, ICurrentContextFifoTaskQueue
    {
        public CurrentContextFifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger) : base(taskScheduler, provider, logger)
        {
        }
    }
}
