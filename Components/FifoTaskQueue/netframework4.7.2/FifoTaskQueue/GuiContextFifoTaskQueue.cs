using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    public class GuiContextFifoTaskQueue : FifoTaskQueue, IGuiContextFifoTaskQueue
    {
        public GuiContextFifoTaskQueue(ILogger logger) : base(TaskShedulerWraper.Create().FromGUIWorker(), logger)
        {
        }
    }
}
