using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using WPF_Usage.ExampleModule.Model;

namespace WPF_Usage.ExampleModule.ViewModel
{
    public interface IExampleViewModel : INotifyPropertyChanged
    {
        public Func<IExampleModel> webContentModel { get; set; }
    }
}
