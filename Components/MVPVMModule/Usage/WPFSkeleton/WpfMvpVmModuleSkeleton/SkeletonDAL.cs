using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMvpVmModuleSkeleton
{
    public class SkeletonDAL : DAL
    {
        public SkeletonDAL(IDomainModels domainModels, IDomainModelFactory domainModelFactory) : base(domainModels, domainModelFactory)
        {
        }
    }
}
