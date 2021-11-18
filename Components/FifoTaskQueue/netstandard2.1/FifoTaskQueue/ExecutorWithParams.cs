using fmacias.Components.FifoTaskQueueAbstract;
using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.FifoTaskQueue
{
    public class ExecutorWithParams<TAction,TArgs>
    {
        private TArgs[] arguments;
        private readonly IActionObserver<TAction> actionObserver;

        private ExecutorWithParams(IActionObserver<TAction> actionObserver, TArgs[] arguments)
        {
            this.actionObserver = actionObserver;
            this.arguments = arguments;
        }
        public static ExecutorWithParams<TAction,TArgs> Create(IActionObserver<TAction> actionObserver, TArgs[] arguments) 
        {
            return new ExecutorWithParams<TAction, TArgs>(actionObserver, arguments);
        }
        public void Execute()
        {
            Action<TArgs> actionOneParam;
            Action<TArgs[]> actionSeveralParams;

            if (this.arguments.Length == 1)
            {
                actionOneParam = actionObserver.GetAction() as Action<TArgs>;
                actionOneParam.Invoke(this.arguments[0]);
            }
            else {
                actionSeveralParams = actionObserver.GetAction() as Action<TArgs[]>;
                actionSeveralParams.Invoke(this.arguments);
            }
        }
    }
}
