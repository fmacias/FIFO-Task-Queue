using System;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IObserver: IObserver<Task>
    {
        void Subscribe(ITasksProvider provider);
        void Unsubscribe();
        ObserverStatus Status { get; set; }
    }
}
