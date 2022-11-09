using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class JobAction<TAction>: Job<TAction>, IJobAction<TAction>
    {
        private Action actionWithoutParams;
        private JobAction()
        {
        }

        public static JobAction<TAction> Create()
        {
            return new JobAction<TAction>();
        } 

        public override IJobRunner Run()
        {
            actionWithoutParams.Invoke();
            return this;
        }

		public override IJobRunner RunAsync()
		{
            Task.Run(async () =>
            {
                Func<Task> func = () => {
                    return Task.Run(() => actionWithoutParams.Invoke());
                };
                await func();
            }).Wait();
            return this;
        }

		public IJob<TAction> Set(TAction action)
        {
            this.action = action;
            this.actionWithoutParams = action as Action;
            return this;
        }

        protected override bool IsAsycn()
        {
            return actionWithoutParams.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }
    }
}
