using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueueDomainModel: IDomainModel
    {
        public string JobName { get; set; }
        public string JobURI { get; set; }
        public string JobStatus { get; set; }
    }
}
