using FifoTaskQueueAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal interface IJobActionArgs<TAction, TArgs>
    {
        IJob<TAction> Set(TAction action, params TArgs[] args);
    }
}