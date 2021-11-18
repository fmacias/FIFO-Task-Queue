using fmacias.Components.FifoTaskQueueAbstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.FifoTaskQueue
{
    public class ExecutorWithParams<TAction>
    {
        private readonly IActionObserver<TAction> actionObserver;

        private ExecutorWithParams(IActionObserver<TAction> actionObserver)
        {
            this.actionObserver = actionObserver;
        }
        public static ExecutorWithParams<TAction> Create(IActionObserver<TAction> actionObserver)
        {
            return new ExecutorWithParams<TAction>(actionObserver);
        }
        public void Execute()
        {
            Action invokable = actionObserver.GetAction() as Action;
            invokable.Invoke();
        }
    }
}
