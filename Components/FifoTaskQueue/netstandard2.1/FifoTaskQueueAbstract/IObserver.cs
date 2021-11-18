using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public interface IObserver: IObserver<Task>
    {
        void Subscribe(ITasksProvider provider);
        void Unsubscribe();
        ObserverStatus Status { get; set; }
        IDisposable Unsubscriber { get; }
        Task ObservableTask { get; }

    }
}
