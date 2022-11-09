using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class GuiContextFifoTaskQueue : FifoTaskQueue, IGuiContextFifoTaskQueue
    {
        public GuiContextFifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger) : base(taskScheduler, provider, logger)
        {
        }
    }
}
