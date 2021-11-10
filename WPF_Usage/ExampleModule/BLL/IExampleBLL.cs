using fmacias;
using fmacias.Components.FifoTaskQueue;
using System;
using System.Collections.Generic;
using System.Text;

namespace WPF_Usage.ExampleModule.BLL
{
    public interface IExampleBLL
    {
        public Func<ITaskQueue> TaskQueueFactory { get; set; }
    }
}
