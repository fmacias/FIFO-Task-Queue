using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public interface ISubscriberFactory
    {
        IActionObserver<TAction, TArgs> Create<TAction, TArgs>(ILogger logger);
    }
}
