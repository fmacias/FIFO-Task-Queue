using fmacias.Components.FifoTaskQueueAbstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public class JobAction<TAction, TArgs> : Job<TAction, TArgs>
    {
        private Action<TArgs> actionOneParam;
        private Action<TArgs[]> actionSeveralParams;

        private JobAction() { }
        public static Job<TAction, TArgs> Create() 
        {
            return new JobAction<TAction, TArgs>();    
        }

        public override void Run()
        {
            if (this.Parameters.Length == 1)
            {
                actionOneParam.Invoke(this.Parameters[0]);
            }
            else
            {
                actionSeveralParams.Invoke(this.Parameters);
            }
        }

        public override IJob<TAction, TArgs> Set(TAction action, params TArgs[] args)
        {
            base.SetAction(action, args);

            if (this.Parameters.Length == 1)
            {
                actionOneParam = GetAction<TArgs>(action);
            }
            else
            {
                actionSeveralParams = GetAction<TArgs[]>(action);
            }

            return this;
        }
    }
}
