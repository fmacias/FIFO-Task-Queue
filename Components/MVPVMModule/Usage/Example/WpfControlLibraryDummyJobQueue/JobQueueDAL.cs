using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueueDAL : DAL
    {
        public JobQueueDAL(IDomainModels domainModels, IDomainModelFactory domainModelFactory) : base(domainModels, domainModelFactory)
        {
        }

        public JobQueueDAL InitializeDummyModels()
        {
            JobQueueDomainModel model1 = CreateDomainModel("http://www.w3.org", "model1");
            JobQueueDomainModel model2 = CreateDomainModel("https://stackoverflow.com/", "model2");
            JobQueueDomainModel model3 = CreateDomainModel("https://www.codeproject.com/", "model3");
            JobQueueDomainModel model4 = CreateDomainModel("https://github.com/fmacias", "model4");
            return this;
        }
        private JobQueueDomainModel CreateDomainModel(string url,string jobName, string jobStatus="Not Running")
        {
            JobQueueDomainModel model= (JobQueueDomainModel)this.domainModelFactory.Create<JobQueueDomainModel>();
            model.JobName = jobName;
            model.JobURI = url;
            model.JobStatus = jobStatus;
            this.AddModel(model);
            return model;
        }
    }
}
