using System;
using System.Collections.Generic;
using System.Text;

namespace WPF_Usage.ExampleModule.View
{
    public interface IExampleView
    {
        public IExamplePresenter ExamplePresenter { get; set; }
    }
}
