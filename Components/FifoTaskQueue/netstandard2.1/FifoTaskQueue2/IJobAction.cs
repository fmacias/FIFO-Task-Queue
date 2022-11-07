using FifoTaskQueueAbstract;
using fmacias.Components.FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public interface IJobAction<TAction>
    {
        IJob<TAction, object> Set(TAction action);
    }
}