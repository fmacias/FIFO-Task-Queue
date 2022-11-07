using System;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public abstract class Job<TAction>: IJob<TAction>
    {
        protected TAction action;
        protected Task defaultAsyncAction = Task.Run(() => { });

        public Action<object> StartAction => (runner) =>
        {
            ((IJobRunner)runner).Run();
        };

        public Action<Task, object> ContinueAction => (task, runner) =>
        {
            ((IJobRunner)runner).Run();
        };

        public bool IsAsync()
        {
            return IsAsycn();
        }

        public abstract IJobRunner Run();
        protected abstract bool IsAsycn();
    }
}
