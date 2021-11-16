using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    public class DomainModelFactory : IDomainModelFactory
    {
        public IDomainModel Create<T>() where T : new()
        {
            return (IDomainModel) new T();
        }
    }
}
