using System;
using System.Collections.Generic;
using System.Text;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IJobDefiner<TAction>
    {
        IJob<TAction> Set<TArgs>(TAction action, TArgs[] args);
    }
}
