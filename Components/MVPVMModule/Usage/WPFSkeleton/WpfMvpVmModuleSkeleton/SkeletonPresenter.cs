using fmacias.Components.EventAggregator;
using fmacias.Components.FifoTaskQueueAbstract;
using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FifoTaskQueueAbstract;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using Unity;

namespace WpfMvpVmModuleSkeleton
{
    public class SkeletonPresenter<TViewType> : Presenter<TViewType>
    {
        private readonly IGuiContextFifoTaskQueue guiContextFifoTaskQueue;
        public SkeletonPresenter(IBLL bll, IViewModel viewModel, IEventAggregator eventAggregator, 
            IGuiContextFifoTaskQueue guiContextFifoTaskQueue) : base(bll, viewModel, eventAggregator)
        {
            this.guiContextFifoTaskQueue = guiContextFifoTaskQueue;
        }
    }
}
