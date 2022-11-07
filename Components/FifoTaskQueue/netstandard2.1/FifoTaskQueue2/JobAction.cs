using fmacias.Components.FifoTaskQueueAbstract;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public class JobAction<TAction> : Job<TAction, object>
    {
        private Action<object> action;

        private JobAction(){}
        public static Job<TAction,object> Create()
        {
            return new JobAction<TAction>();
        }

        public override void Run()
        {
            action.Invoke(this);
        }
        public override IJob<TAction, object> Set(TAction action, params object[] args)
        {
            base.SetAction(action);
            this.action = GetAction<object>(action);
            return this;
        }
    }
}
