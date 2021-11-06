using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Unity;
using WPF_Usage.ExampleModule.Model;

namespace WPF_Usage.ExampleModule.ViewModel
{
    public class ExampleViewModel : IExampleViewModel
    {
        [Dependency]
        public Func<IExampleModel> webContentModel { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
