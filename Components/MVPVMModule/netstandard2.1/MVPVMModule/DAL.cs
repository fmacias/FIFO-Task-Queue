using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    /// <summary>
    /// Data Access Layer
    /// </summary>
    public abstract class DAL : IDAL
    {
        protected readonly IDomainModels domainModels;
        protected readonly IDomainModelFactory domainModelFactory;

        protected DAL(IDomainModels domainModels, IDomainModelFactory domainModelFactory)
        {
            this.domainModels = domainModels;
            this.domainModelFactory = domainModelFactory;
        }

        public IDomainModels DomainModels => domainModels;

        public IDomainModelFactory DomainModelFactory => domainModelFactory;

        public IDAL AddModel(IDomainModel Model)
        {
            domainModels.Models.Add(Model);
            return this;
        }
    }
}
