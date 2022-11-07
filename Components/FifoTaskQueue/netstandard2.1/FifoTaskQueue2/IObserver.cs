using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FifoTaskQueue2
{
    public interface IObserver: IObserver<Task>
    {
        void Subscribe(ITasksProvider provider);
        void Unsubscribe();
        ObserverStatus Status { get; set; }
        IDisposable Unsubscriber { get; }
        Task ObservableTask { get; }
        bool LastProcessingTaskHasBeenFinalized();
        void PublishEvents();
        Type getJobActionType();

        object[] getJobParamters();
        Type getArgsType();
    }
}
