using System;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public abstract class Job<TAction>: IJob<TAction>
    {
        protected TAction action;
        protected Task defaultAsyncAction = Task.Run(() => { });

        public bool IsAsync()
        {
            return IsAsycn();
        }

        public abstract IJobRunner Run();
        public abstract IJobRunner RunAsync();

		protected abstract bool IsAsycn();
    }
}
