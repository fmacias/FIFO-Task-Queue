using FifoTaskQueueAbstract;
using fmacias.Components.FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public interface IJobActionArgs<TAction, TArgs>
    {
        IJob<TAction, TArgs> Set(TAction action, params TArgs[] args);
    }
}