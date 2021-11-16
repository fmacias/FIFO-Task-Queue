using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueueAbstract
{
    public interface ITaskObserver<T>: IObserver<T>
    {
        IDisposable Unsubscriber { get; }
    }
}
