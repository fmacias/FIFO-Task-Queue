using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    /// <summary>
    /// Data Access Layer minimal defintion
    /// </summary>
    public interface IDAL{
        IDomainModels DomainModels { get; }
        IDAL AddModel(IDomainModel Model);
        IDomainModelFactory DomainModelFactory { get; }
    }
}
