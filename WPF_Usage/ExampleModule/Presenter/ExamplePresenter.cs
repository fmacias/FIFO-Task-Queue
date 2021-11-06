using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using WPF_Usage.ExampleModule.BLL;
using WPF_Usage.ExampleModule.ViewModel;

namespace WPF_Usage.ExampleModule.Presenter
{
    public class ExamplePresenter : IExamplePresenter
    {
        [Dependency]
        public IExampleBLL ExampleBLL { get; set; }
        [Dependency]
        public IExampleViewModel ExampleViewModel { get; set; }

    }
}
