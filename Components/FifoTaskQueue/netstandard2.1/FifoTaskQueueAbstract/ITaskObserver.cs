using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public interface ITaskObserver: IObserver
    {
        Task<bool> TaskStatusCompletedTransition { get; }
        
    }
}
