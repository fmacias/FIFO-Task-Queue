using fmacias.Components.EventAggregator;
using fmacias.Components.FifoTaskQueueAbstract;
using fmacias.Components.MVPVMModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FifoTaskQueueAbstract;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using Unity;
using WpfControlProgressBar;

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueuePresenter<TViewType> : Presenter<TViewType>
    {
        private readonly IGuiContextFifoTaskQueue guiContextFifoTaskQueue;
        private readonly JobQueueBLL jobQueueBLL;
        private readonly JobQueueViewModel jobQueueViewModel;
        private DummyFifoJobQueue jobqueueView;

        public JobQueuePresenter(IBLL bll, IViewModel viewModel, IEventAggregator eventAggregator,
            IGuiContextFifoTaskQueue guiContextFifoTaskQueue) : base(bll, viewModel, eventAggregator)
        {
            this.guiContextFifoTaskQueue = guiContextFifoTaskQueue;
            this.jobQueueBLL = bll as JobQueueBLL;
            this.jobQueueViewModel = viewModel as JobQueueViewModel;
        }
        public override IPresenter<TViewType> SetView(TViewType view)
        {
            base.SetView(view);
            jobqueueView = view as DummyFifoJobQueue;
            SubscribeEvents();
            List<JobQueueDomainModel> queues = jobQueueBLL.LoadDefaultData();
            UpdateModelView(queues);
            DrawProgressBars(queues);
            return this;
        }
        private void UpdateModelView(List<JobQueueDomainModel> queues)
        {
            queues.ForEach((JobQueueDomainModel) =>
            {
                jobQueueViewModel.AddJob(JobQueueDomainModel);
            });
        }
        private void DrawProgressBars(List<JobQueueDomainModel> queues)
        {
            queues.ForEach((JobQueueDomainModel) =>
            {
                FifoProgressBar progressBar = new FifoProgressBar();
                progressBar.DataContext = JobQueueDomainModel;
                progressBar.JobURI = JobQueueDomainModel.JobURI;
                progressBar.JobStatus = JobQueueDomainModel.JobStatus;
                jobqueueView.Queues.Children.Add(progressBar);
            });
        }
        private IUIEventSubscriptor NewUISubscription()
        {
            return this.eventAggregator.UIEventSubscriptorFactory.Create(this.eventAggregator);
        }
        private void SubscribeUIObject<THandler>(object button, string eventName, THandler handlerDelegate)
        {
            NewUISubscription().AddEventHandler<THandler>(handlerDelegate, eventName, button);
        }
        private void SubscribeEvents()
        {
            SubscribeUIObject<RoutedEventHandler>(jobqueueView.btnStart, "Click", async (object sender, RoutedEventArgs e) => {
                await guiContextFifoTaskQueue.Complete();
                UIElementCollection progressBarCollection = jobqueueView.Queues.Children;
                for (int i = 0; i < progressBarCollection.Count - 1; i++)
                {
                    string uri = (progressBarCollection[i] as FifoProgressBar).JobURI;
                    ProgressBar progressBar = ExtractProcessBar(progressBarCollection, i);
                }
                await guiContextFifoTaskQueue.CancelAfter(10000);
            });
            SubscribeUIObject<RoutedEventHandler>(jobqueueView.btnStop, "Click", (object sender, RoutedEventArgs e) => {
                guiContextFifoTaskQueue.CancelExecution();
            });
        }

        private void ProgressBar(ProgressBar progressBar)
        {
            Task.Factory.StartNew((progressBar) =>
            {
                //todo
            }, progressBar, guiContextFifoTaskQueue.CancellationToken, TaskCreationOptions.None, guiContextFifoTaskQueue.TaskScheduler);
        }

        private static ProgressBar ExtractProcessBar(UIElementCollection progressBarCollection, int i)
        {
            FifoProgressBar fifoProgressBar = progressBarCollection[i] as FifoProgressBar;
            ProgressBar progressBar = (ProgressBar)fifoProgressBar.FindName("pbStatus");
            return progressBar;
        }

        private void IncreaseProgressBar(HttpContent content, ProgressBar progressBar)
        {
            Task<byte[]> currentlyLoadedBytes = content.ReadAsByteArrayAsync();
            var contentLength = content.Headers.ContentLength;
            var asyncState = currentlyLoadedBytes.AsyncState;

            if (asyncState == null || contentLength == null)
                IncreaseProgressBar(content, progressBar);

            int loadedBytes = (asyncState as byte[]).Length;
            int bytesToLoad = BitConverter.GetBytes((long)contentLength).Length;
            if (loadedBytes < bytesToLoad)
            {
                progressBar.Value = Convert.ToDouble(loadedBytes);
                IncreaseProgressBar(content, progressBar);
            }
            progressBar.Value = Convert.ToDouble(bytesToLoad);
        }

        private void ProcessHttpRequestJob(string url)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = httpRequestContent(client, url);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                //cancel queue
                //show status Error
            }
        }

        private HttpResponseMessage httpRequestContent(HttpClient client, string url)
        {
            var requestMessage = new HttpRequestMessage();
            requestMessage.RequestUri = new Uri(url);
            HttpResponseMessage response = client.Send(requestMessage, HttpCompletionOption.ResponseContentRead, guiContextFifoTaskQueue.CancellationToken);
            return response;
        }

        public override void UnsubscribeAll()
        {
            base.UnsubscribeAll();
        }
    }
}
