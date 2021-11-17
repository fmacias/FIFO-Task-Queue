using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueueBLL : BLL
    {
        private readonly JobQueueDAL jobQueueDal;
        public JobQueueBLL(IDAL dal) : base(dal)
        {
            jobQueueDal = dal as JobQueueDAL;
        }
        public List<JobQueueDomainModel> LoadDefaultData()
        {
            jobQueueDal.InitializeDummyModels();
            return jobQueueDal.DomainModels.Models.Cast<JobQueueDomainModel>().ToList();
        }
        public List<JobQueueDomainModel> Models=> jobQueueDal.DomainModels.Models.Cast<JobQueueDomainModel>().ToList();
    }
}
