using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    abstract public class ViewModel : IViewModel
    {
        public abstract event PropertyChangedEventHandler PropertyChanged;

        protected abstract void NotifyPropertyChanged([CallerMemberName] String propertyName = "");
    }
}
