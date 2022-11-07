using System;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IJobRunner
    {
        IJobRunner Run();
        Action<object> StartAction { get; }
        Action<Task, object> ContinueAction { get; }
        bool IsAsync();
    }
}
