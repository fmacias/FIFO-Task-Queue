using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    public interface IDomainModelFactory
    {
        IDomainModel Create<T>() where T : new();
    }
}
