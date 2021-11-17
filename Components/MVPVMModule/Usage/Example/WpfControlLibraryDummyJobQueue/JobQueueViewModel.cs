using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueueViewModel : ViewModel
    {
        private ObservableCollection<JobQueueDomainModel> observableColletion = new ObservableCollection<JobQueueDomainModel>();

        public override event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<JobQueueDomainModel> Jobs {
            get { return observableColletion; }
        }
        protected override void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void AddJob(JobQueueDomainModel domainModel)
        {
            observableColletion.Add(domainModel);
        }
    }
}
