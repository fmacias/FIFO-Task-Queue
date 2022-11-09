using System.Threading;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IActionObserver<TAction>: ITaskObserver
    {
        IActionObserver<TAction> SetJob(IJob<TAction> job);
        IActionObserver<TAction> OnCompleteCallback(IProcessEvent.ProcessEventHandler handler);
        IActionObserver<TAction> OnErrorCallback(IProcessEvent.ProcessEventHandler handler);
        string Name { get; set; }
    }
}

