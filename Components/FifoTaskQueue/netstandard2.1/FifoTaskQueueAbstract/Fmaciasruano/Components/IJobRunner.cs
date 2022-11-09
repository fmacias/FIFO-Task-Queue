using System;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IJobRunner
    {
        IJobRunner Run();
        IJobRunner RunAsync();
        bool IsAsync();
    }
}
