using System.Threading;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface ITaskObserver : IObserver
    {
        //Task<bool> TaskStatusCompletedTransition { get; }
        IJobRunner Runner { get; }
        Task<bool> TaskStatusFinishedTransition { get; }
        Task<IJobRunner> RunningTask {get;}
        CancellationTokenSource CancellationTokenSource { get; set; }
        int MaximumExecutionTime { get; set; }
    }
}

