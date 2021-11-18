using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public enum ObserverStatus
    {
        Created = 0,
        Observed = 1,
        Completed = 2,
        CompletedWithErrors = 3,
        ExecutionTimeExceded = 4,
        Canceled = 5,
        Unkonown = 6
    }
}
