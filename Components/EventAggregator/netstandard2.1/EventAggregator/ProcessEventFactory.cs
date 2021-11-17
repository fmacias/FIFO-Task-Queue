using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class ProcessEventFactory : IProcessEventFactory
    {
        public IProcessEvent Create<T>() where T : new()
        {
            return (IProcessEvent) new T();
        }
    }
}
