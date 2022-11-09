using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class JobActionArgs<TAction,TArgs> : Job<TAction>, IJobActionArgs<TAction, TArgs>
    {
        private Action<TArgs> actionOneParam;
        private Action<TArgs[]> actionSeveralParams;
        private TArgs[] args;

        private JobActionArgs()
        {
        }

        public static JobActionArgs<TAction, TArgs> Create()
        {
            return new JobActionArgs<TAction, TArgs>();
        } 

        public override IJobRunner Run()
        {
            if (this.args.Length == 1)
                actionOneParam.Invoke(this.args[0]);
            else
                actionSeveralParams.Invoke(this.args);

            return this;
        }
        public override IJobRunner RunAsync()
        {
            Task.Run(async () =>
            {
                Func<Task> func = () => {
                    
                    return Task.Run(() => {
                        if (this.args.Length == 1)
                            actionOneParam.Invoke(this.args[0]);
                        else
                            actionSeveralParams.Invoke(this.args);
                    });
                };
                await func();
            }).Wait();

            return this;
        }

        public IJob<TAction> Set(TAction action, params TArgs[] args)
        {
            this.action = action;
            this.args = args;

            if (args.Length == 1)
                actionOneParam = action as Action<TArgs>;
            else
                actionSeveralParams = action as Action<TArgs[]>;

            IsAsycn();

            return this;
        }
       
        protected override bool IsAsycn()
        {
            if (this.args.Length == 1)
                return actionOneParam.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);

            return actionSeveralParams.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }
	}
}
