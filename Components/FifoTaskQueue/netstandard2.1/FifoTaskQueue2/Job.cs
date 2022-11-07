using fmacias.Components.FifoTaskQueueAbstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public abstract class Job<TAction,TArgs>: IJob<TAction,TArgs>, IJobRunner
    {
        private TAction actionType;
        private TArgs actionArgumentsType;
        private TArgs[] actionParameters;

        public TAction ActionType => actionType;

        public TArgs Args => actionArgumentsType;

        public TArgs[] Parameters => actionParameters;

        Action<object> IJobRunner.StartAction => StartAction();

        Action<Task, object> IJobRunner.ContinueAction => ContinueAction();

        public abstract void Run();
        protected IJob<TAction, TArgs> SetAction(TAction action, TArgs[] args = null)
        {
            this.actionType = action;
            this.actionParameters = args;
            return this;
        }
        protected Action<T> GetAction<T>(TAction action)
        {
            return action as Action<T>;
        }
        private Action<object> StartAction()
        {
            Action<object> actionObject = (args) => {
                IJobRunner runner = args as IJobRunner;
                runner.Run();
            };
            return actionObject;
        }
        private Action<Task, object> ContinueAction()
        {
            Action<Task, object> actionTask = (task, args) =>
            {
                IJobRunner runner = args as IJobRunner;
                runner.Run();
            };
            return actionTask;
        }

        public abstract IJob<TAction, TArgs> Set(TAction action, params TArgs[] args);
    }
}
