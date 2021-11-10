using System;
using System.Collections.Generic;
using System.Text;

namespace MVPVMAbstract
{
    public interface IEventAggregator<T>: IObservable<T>
    {
        void Publish<T>(T EventData);
    }
}
