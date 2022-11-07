using EventAggregatorAbstract.Fmaciasruano.Components;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IActionObserver<TAction>: ITaskObserver
    {
        IActionObserver<TAction> SetJob(IJob<TAction> job);
        IActionObserver<TAction> OnCompleteCallback(IProcessEvent.ProcessEventHandler handler);
        IActionObserver<TAction> OnErrorCallback(IProcessEvent.ProcessEventHandler handler);
    }
}

