using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public interface IActionObserver<TAction>: IObserver, ITaskObserver
    {
        public delegate void CompleteCallBackEventHandler(object sender);
        public delegate void ErrorCallBackEventHandler(object sender);
        IActionObserver<TAction> SetAction(TAction action);
        TAction GetAction();
        IActionObserver<TAction> OnCompleteCallback(CompleteCallBackEventHandler completeCallbackDelegate);
        IActionObserver<TAction> OnErrorCallback(ErrorCallBackEventHandler errorCallbackDelegate);
    }
}
