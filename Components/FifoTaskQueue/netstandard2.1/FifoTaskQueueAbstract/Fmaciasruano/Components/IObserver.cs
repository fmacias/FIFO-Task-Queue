using System;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IObserver: IObserver<Task<IJobRunner>>
    {
        void Subscribe(ITasksProvider provider);
        void Unsubscribe();
        ObserverStatus Status { get; set; }
    }
}
