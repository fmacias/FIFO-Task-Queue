using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface ITaskObserver: IObserver
    {
        //Task<bool> TaskStatusCompletedTransition { get; }
        IJobRunner Runner { get; }
    }
}

