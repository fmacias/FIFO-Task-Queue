using System;
using System.Threading.Tasks;
using EventAggregatorAbstract.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class TaskObserver<TAction> : IActionObserver<TAction>
    {
        private const int MaximalTaskWatcherElapsedTimeMs = 600000;
        private readonly IEventAggregator eventAggregator;
        private IProcessEventSubscriptor onCompletedEventSubscriptor;
        private IProcessEventSubscriptor onErrorCallbackEventSubscriptor;
        private readonly ILogger logger;
        private IJob<TAction> job;
        private IDisposable unsubscriber;
        private TaskObserver(IEventAggregator eventAggregator, ILogger logger)
        {
            this.eventAggregator = eventAggregator;
            onCompletedEventSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            onErrorCallbackEventSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            SubscriptorHelper.AddUnicEventHandler(defaultCallbackSubscription, onCompletedEventSubscriptor, 
                eventAggregator,this);
            SubscriptorHelper.AddUnicEventHandler(defaultCallbackSubscription, onErrorCallbackEventSubscriptor,
                eventAggregator, this);
            this.logger = logger;
            //TaskStatusCompletedTransition = Task.Run(() => false);
            Status = ObserverStatus.Created;
        }
        internal static TaskObserver<TAction> Create(IEventAggregator eventAggregator, ILogger logger)
        {
            return new TaskObserver<TAction>(eventAggregator,logger);
        }
        //public Task<bool> TaskStatusCompletedTransition { get; private set; }
        public IJobRunner Runner => job;
        public ObserverStatus Status { get; set; }
        public void OnCompleted()
        {
            onCompletedEventSubscriptor.Publish();
        }
        public void OnError(Exception error)
        {
            if (error.GetType() == typeof(TaskCanceledException) || error.GetType() == typeof(AggregateException))
                Status = ObserverStatus.Canceled;

            logger.Debug("Task Observer Error: " + error.ToString());
            onErrorCallbackEventSubscriptor.Publish();
        }
        public void OnNext(Task value)
        {
            Status = ObserverStatus.Observing;
            logger.Debug($"Task id: {value.Id} Will be observe. State: {value.Status}");
            PollingTaskStatusTransition(value);
        }
        public IActionObserver<TAction> SetJob(IJob<TAction> job)
        {
            this.job = job;
            return this;
        }
        public void Subscribe(ITasksProvider provider)
        {
            unsubscriber = provider.Subscribe(this);
        }
        public void Unsubscribe()
        {
            onCompletedEventSubscriptor.Unsubscribe();
            onErrorCallbackEventSubscriptor.Unsubscribe();
            unsubscriber?.Dispose();
        }

        public IActionObserver<TAction> OnCompleteCallback(IProcessEvent.ProcessEventHandler handler)
        {
            this.onCompletedEventSubscriptor.Unsubscribe();
            this.onCompletedEventSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            SubscriptorHelper.AddUnicEventHandler(handler, onCompletedEventSubscriptor,
                eventAggregator, this);
            return this;
        }

        public IActionObserver<TAction> OnErrorCallback(IProcessEvent.ProcessEventHandler handler)
        {
            this.onErrorCallbackEventSubscriptor.Unsubscribe();
            this.onErrorCallbackEventSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            SubscriptorHelper.AddUnicEventHandler(handler, onErrorCallbackEventSubscriptor,
                eventAggregator, this);
            return this;
        }

        private void PollingTaskStatusTransition(Task task)
        {
            //TaskStatusCompletedTransition.Wait();
            //TaskStatusCompletedTransition.Dispose();
            object[] asyncParams = {task, this};

            Task.Factory.StartNew((o) =>
            {
                object[] inputParams = (object[]) o;
                Task runnigTask = (Task) inputParams[0];
                IObserver observer = (IObserver) inputParams[1];
                long executionElapsedTime = WatchTaskCompletationPolling(runnigTask);

                observer.Status = TaskObserverStatusMapping(runnigTask);

            }, asyncParams).Wait();

            bool IsTaskBeingProcessed(Task runningTask)
            {
                return runningTask.IsCompleted || runningTask.IsCanceled || runningTask.IsFaulted;
            }

            static bool MaximalElapsedTimeExceded(System.Diagnostics.Stopwatch watch)
            {
                return watch.ElapsedMilliseconds > MaximalTaskWatcherElapsedTimeMs;
            }

            long WatchTaskCompletationPolling(Task runningTask)
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                TaskStatus currentStatus = runningTask.Status;
                logger.Debug($"Task id: {runningTask.Id} initial status {runningTask.Status}");

                while (!(IsTaskBeingProcessed(runningTask)) && (!MaximalElapsedTimeExceded(watch)))
                {
                    if (currentStatus != runningTask.Status)
                    {
                        logger.Debug($"Task id: {runningTask.Id} Status transition to {runningTask.Status}");
                        currentStatus = runningTask.Status;
                    }
                }

                long executionTime = watch.ElapsedMilliseconds;
                logger.Debug(
                    $"Task id: {runningTask.Id},  final status {runningTask.Status}, Duration: {executionTime}");
                watch.Stop();
                return executionTime;
            }
            ObserverStatus TaskObserverStatusMapping(Task task)
            {
                ObserverStatus status;
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        status = ObserverStatus.Completed;
                        break;
                    case TaskStatus.Running:
                        status = ObserverStatus.ExecutionTimeExceeded;
                        break;
                    case TaskStatus.Faulted:
                        status = ObserverStatus.CompletedWithErrors;
                        break;
                    case (TaskStatus.Canceled):
                        status = ObserverStatus.Canceled;
                        break;
                    default:
                        status = ObserverStatus.Unknown;
                        break;
                }

                return status;
            }
        }
        private void defaultCallbackSubscription(object sender) { }
    }
}
