using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public interface ITaskObserver<T>: IObserver<T>
    {
        public delegate void CompleteCallBackEventHandler(object sender);
        public delegate void ErrorCallBackEventHandler(object sender);
        IDisposable Unsubscriber { get; }
        Task ObservableTask { get; }
        TaskObserverStatus Status { get; }
        Task<bool> TaskStatusCompletedTransition { get; }
        ITaskObserver<T> OnCompleteCallback(CompleteCallBackEventHandler completeCallbackDelegate);
        ITaskObserver<T> OnErrorCallback(ErrorCallBackEventHandler errorCallbackDelegate);
        void OnNext(Task value);
        void Subscribe(ITasksProvider provider);
        void Unsubscribe();
    }
}
