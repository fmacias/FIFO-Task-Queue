using System;
using System.ComponentModel;

namespace fmacias.Components.MVPVMModule
{
    public interface IViewModel : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChanged;
    }
}
