using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    internal class SubscriberFactory : ISubscriberFactory
    {
        public IActionObserver<TAction, TArgs> Create<TAction, TArgs>(ILogger logger)
        {
            throw new NotImplementedException();
        }
    }
}
