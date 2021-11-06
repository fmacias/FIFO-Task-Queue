using System;
using System.Collections.Generic;
using System.Text;
using WPF_Usage.ExampleModule.BLL;
using WPF_Usage.ExampleModule.ViewModel;

namespace WPF_Usage
{
    public interface IExamplePresenter
    {
        public IExampleBLL ExampleBLL { get; set; }
        public IExampleViewModel ExampleViewModel { get; set; }
    }
}
