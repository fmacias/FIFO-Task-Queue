using System;
using System.Collections.Generic;
using System.Text;
using fmacias;
using Unity;

namespace WPF_Usage.ExampleModule.BLL
{
    /// <summary>
    /// Web Content business Logic Layer
    /// </summary>
    public class ExampleBLL : IExampleBLL
    {
        [Dependency]
        public Func<ITaskQueue> TaskQueueFactory { get; set; }

    }
}
