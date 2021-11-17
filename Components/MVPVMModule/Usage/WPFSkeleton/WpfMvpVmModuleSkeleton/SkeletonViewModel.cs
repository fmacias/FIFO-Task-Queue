using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfMvpVmModuleSkeleton
{
    public class SkeletonViewModel : ViewModel
    {
        public override event PropertyChangedEventHandler PropertyChanged;

        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            throw new NotImplementedException();
        }
    }
}
