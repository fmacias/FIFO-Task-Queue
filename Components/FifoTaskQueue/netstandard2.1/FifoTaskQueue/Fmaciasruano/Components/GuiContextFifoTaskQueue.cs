using System.Threading.Tasks;
using EventAggregatorAbstract.Fmaciasruano.Components;
using FifoTaskQueueAbstract;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class GuiContextFifoTaskQueue : FifoTaskQueue, IGuiContextFifoTaskQueue
    {
        public GuiContextFifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger, IEventAggregator eventAggregator) : base(taskScheduler, provider, logger, eventAggregator)
        {
        }
    }
}
