using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    public abstract class DomainModels : IDomainModels
    {
        protected readonly List<IDomainModel> models= new List<IDomainModel>();
        public List<IDomainModel> Models => models;
    }
}
