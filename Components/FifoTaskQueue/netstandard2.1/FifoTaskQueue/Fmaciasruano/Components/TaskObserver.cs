using System;
using System.Threading;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class TaskObserver<TAction> : IActionObserver<TAction>
    {
        public const int MAXIMAL_EXECUTION_TIME = 60000;
        private ICallbackProcessEvent onCompletedEventSubscriptor;
        private ICallbackProcessEvent onErrorCallbackEventSubscriptor;
        private readonly ILogger logger;
        private IJob<TAction> job;
        private IDisposable unsubscriber;
        private Task<bool> taskStatusFinishedTransition;
        private Task<IJobRunner> runningTask;
        private CancellationTokenSource cancelationTokenSource;
        private int maximalExecutionTime;
        private TaskObserver(ILogger logger)
        {
            onCompletedEventSubscriptor = CallbackProcessEvent.Create();
            onErrorCallbackEventSubscriptor = CallbackProcessEvent.Create();
            onCompletedEventSubscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(defaultCallbackSubscription);
            onErrorCallbackEventSubscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(defaultCallbackSubscription);
            this.logger = logger;
            Status = ObserverStatus.Created;
            this.taskStatusFinishedTransition = Task.Run(() => { return false; });
            maximalExecutionTime = MAXIMAL_EXECUTION_TIME;
        }
        internal static TaskObserver<TAction> Create(ILogger logger)
        {
            return new TaskObserver<TAction>(logger);
        }
        //public Task<bool> TaskStatusCompletedTransition { get; private set; }
        public IJobRunner Runner => job;
        public ObserverStatus Status { get; set; }

		public Task<bool> TaskStatusFinishedTransition => taskStatusFinishedTransition;

		public string Name { get; set; }

		public Task<IJobRunner> RunningTask => runningTask;

		public CancellationTokenSource CancellationTokenSource { get => cancelationTokenSource; set => cancelationTokenSource = value; }
		public int MaximumExecutionTime { 
            get => maximalExecutionTime;
            set 
            {
                if (value < 0)
				{
                    throw new FifoTaskQueueWorkflowException("Maximal Execution Time of observed task cann not be null");
				}
                maximalExecutionTime = value;
            }  
        }

		public void OnCompleted()
        {
            onCompletedEventSubscriptor.Publish();
            this.runningTask?.Wait();
            this.Unsubscribe();
        }
        public void OnError(Exception error)
        {
            if (!this.TaskCanceledOrFaultedOnWaitingAtOnCompleted(error))
			{
                this.runningTask?.Wait();
            }
            
            logger.Debug($"OnError -> Objerver:{Name}, Objserver Status: {Status}, Task Status:{runningTask.Status} Task Observer Error: {error.ToString()}");
            onErrorCallbackEventSubscriptor.Publish();
            this.Unsubscribe();
        }
        public void OnNext(Task<IJobRunner> value)
        {
            runningTask = value;
            Status = ObserverStatus.Observing;
            logger.Debug($"Observer: {Name}, State:{Status} Task id: {value.Id} Will be observe. State: {value.Status}");
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
            onCompletedEventSubscriptor.RemoveEventHandler();
            onErrorCallbackEventSubscriptor.RemoveEventHandler();
            unsubscriber?.Dispose();
        }

        public IActionObserver<TAction> OnCompleteCallback(IProcessEvent.ProcessEventHandler handler)
        {
            this.onCompletedEventSubscriptor.RemoveEventHandler();
            this.onCompletedEventSubscriptor = CallbackProcessEvent.Create();
            this.onCompletedEventSubscriptor.Sender = this;
            this.onCompletedEventSubscriptor.AddEventHandler(handler);
            return this;
        }

        public IActionObserver<TAction> OnErrorCallback(IProcessEvent.ProcessEventHandler handler)
        {
            this.onErrorCallbackEventSubscriptor.RemoveEventHandler();
            this.onErrorCallbackEventSubscriptor = CallbackProcessEvent.Create();
            this.onErrorCallbackEventSubscriptor.Sender = this;
            this.onErrorCallbackEventSubscriptor.AddEventHandler(handler);
            return this;
        }

        private void PollingTaskStatusTransition(Task<IJobRunner> task)
        {
            taskStatusFinishedTransition.Wait();
            taskStatusFinishedTransition.Dispose();
            object[] asyncParams = {task, this};

            taskStatusFinishedTransition = Task.Factory.StartNew((o) =>
            {
                bool processed = false;
                object[] inputParams = (object[]) o;
                Task<IJobRunner> runnigTask = (Task<IJobRunner>) inputParams[0];
                ITaskObserver observer = (ITaskObserver) inputParams[1];
                observer.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                long executionTime = WatchTaskCompletationPolling(runnigTask, observer);
                observer.Status = TaskObserverMappingAfterObservation(runnigTask, executionTime);

                if (observer.Status == ObserverStatus.ExecutionTimeExceeded)
				{
                    observer.CancellationTokenSource.Cancel();
				}
                observer.CancellationTokenSource.Token.ThrowIfCancellationRequested();
                logger.Debug($"Final State: Observer:{Name}, Status:{Status}, Task id: {runningTask.Id},  Task State: {runningTask.Status}");
                processed = true;
                return processed;
            }, asyncParams);

            bool IsTaskFinalized(Task runningTask)
            {
                return runningTask.IsCompleted || runningTask.IsCanceled || runningTask.IsFaulted;
            }

            static bool MaximalElapsedTimeExceded(System.Diagnostics.Stopwatch watch, ITaskObserver observer)
            {
                return watch.ElapsedMilliseconds > observer.MaximumExecutionTime;
            }

            long WatchTaskCompletationPolling(Task runningTask, ITaskObserver observer)
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                TaskStatus currentStatus = runningTask.Status;
                logger.Debug($"Observer:{Name},Status:{Status} Task id: {runningTask.Id} initial status {runningTask.Status}");

                while (!(IsTaskFinalized(runningTask)) && (!MaximalElapsedTimeExceded(watch, observer)))
                {
                    if (currentStatus != runningTask.Status)
                    {
                        logger.Debug($"Observer:{Name},Status:{Status} Task id: {runningTask.Id} Status transition to {runningTask.Status}");
                        currentStatus = runningTask.Status;
                    }
                }
                long executionTime = watch.ElapsedMilliseconds;
                logger.Debug($"Observer:{Name}, Status:{Status}, Task id: {runningTask.Id},  Task Status {runningTask.Status}, Duration: {executionTime}");
                watch.Stop();
                return executionTime;
            }
            ObserverStatus TaskObserverMappingAfterObservation(Task task, long executionTime)
            {
                ObserverStatus status;

                if (executionTime > this.maximalExecutionTime)
				{
                    status = ObserverStatus.ExecutionTimeExceeded;
				}
				else
				{
                    switch (task.Status)
                    {
                        case TaskStatus.Created:
                            status = ObserverStatus.Observing;
                            break;
                        case TaskStatus.WaitingForActivation:
                            status = ObserverStatus.Observing;
                            break;
                        case TaskStatus.WaitingToRun:
                            status = ObserverStatus.Observing;
                            break;
                        case TaskStatus.Running:
                            status = ObserverStatus.Observing;
                            break;
                        case TaskStatus.WaitingForChildrenToComplete:
                            status = ObserverStatus.Observing;
                            break;
                        case TaskStatus.RanToCompletion:
                            status = ObserverStatus.Completed;
                            break;
                        case TaskStatus.Canceled:
                            status = ObserverStatus.Canceled;
                            break;
                        case TaskStatus.Faulted:
                            status = ObserverStatus.CompletedWithErrors;
                            break;
                        default:
                            status = ObserverStatus.Unknown;
                            break;
                    }
                }


                return status;
            }
        }
        private void defaultCallbackSubscription(object sender) { }
        private bool TaskCanceledOrFaultedOnWaitingAtOnCompleted(Exception error)
		{
            return error.GetType() == typeof(TaskCanceledException) || error.GetType() == typeof(AggregateException) || error.GetType() == typeof(Exception);
        }
    }
}
