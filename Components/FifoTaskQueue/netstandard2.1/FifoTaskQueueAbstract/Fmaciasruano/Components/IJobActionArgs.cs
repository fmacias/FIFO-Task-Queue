using System;
using System.Collections.Generic;
using System.Text;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IJobActionArgs<TAction,TArgs>:IJob<TAction>
    {
        IJob<TAction> Set(TAction action, params TArgs[] args);
    }
}
