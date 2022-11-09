using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class FifoTaskQueue: ITaskQueue
    {
        private readonly TaskScheduler taskScheduler;
        private readonly ITasksProvider tasksProvider;
        private readonly ILogger logger;
        private CancellationTokenSource cascadeCancellationTokenSource;
        private bool cascadeCancelation = true;
        private Task<IJobRunner> currentJobRunner;
        private int jobMaximalExecutionTime = TaskObserver<IJobRunner>.MAXIMAL_EXECUTION_TIME;

        #region Constructor

        protected FifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger)
        {
            this.taskScheduler = taskScheduler;
            this.tasksProvider = provider;
            this.logger = logger;
        }
        
        public static FifoTaskQueue Create(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger)
        {
            return new FifoTaskQueue(taskScheduler, provider, logger);
        }

        #endregion
        public IActionObserver<TAction> Enqueue<TAction>(TAction action)
        {
            return SubscribeActionJob(action);
        }

        public IActionObserver<TAction> Enqueue<TAction,TArgs>(TAction action, params TArgs[] args)
        {
            return SubscribeActionJob(action, args);
        }
        public ITaskQueue Dequeue()
        {
            if (Provider.Subscriptions.Length == 0)
                return this;

            if (IsTaskObserverAtWork())
            {
                ITaskObserver observer = this.GetLastObserver();
                Task<IJobRunner> previousRunner = this.currentJobRunner;
                this.currentJobRunner = this.Continue(previousRunner, observer);
                TaskObserverCallbacks(observer, this.currentJobRunner);
            }
            else
            {
                ITaskObserver observer = Provider.Subscriptions.ToList().First();
                this.currentJobRunner = this.Start(observer);
                TaskObserverCallbacks(observer, this.currentJobRunner);
            }
            return this;
        }

        /// <summary>
        /// Awaitable method to await processing the queue whenever is required at async methods.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Complete()
        {
            List<bool> observers = await Provider.CompleteQueueObservation();
            return !(Array.IndexOf(observers.ToArray(), false) > -1);
        }

        public ITaskQueue CancelAfter(int miliseconds)
        {
            if (CascadeCancelation)
			{
                CancellationTokenSource.CancelAfter(miliseconds);
			}
			else
			{
                ITaskObserver observer = Provider.Subscriptions.ToList().LastOrDefault(o => o.Status == ObserverStatus.Observing);

                if (observer != null)
				{
                    observer.CancellationTokenSource.CancelAfter(miliseconds);
				}
			}
            
            return this;
        }
        
        public void CancelExecution()
        {
            CancellationTokenSource.Cancel();
        }

        public Task<IJobRunner> Start(ITaskObserver taskObserver)
        {
            CancellationTokenSource cancellationTokenSource = this.CancellationTokenSource;
            Task<IJobRunner> taskRunner = Task<IJobRunner>.Factory.StartNew((obj) =>
            {
                ITaskObserver observer = (ITaskObserver)obj;
                return observer.Runner.Run();
            }, taskObserver, cancellationTokenSource.Token, TaskCreationOptions.None, taskScheduler);
            StartObservingTask(taskObserver, taskRunner, cancellationTokenSource);
            return taskRunner;
        }
        public Task<IJobRunner> Continue(Task<IJobRunner> previousTask, ITaskObserver taskObserver)
        {
            CancellationTokenSource cancellationTokenSource = this.CancellationTokenSource;
            Task<IJobRunner> taskRunner = previousTask.ContinueWith(
                (task, obj) =>
                {
                    ITaskObserver observer = (ITaskObserver)obj;
                    return observer.Runner.Run();
                }, taskObserver, cancellationTokenSource.Token, TaskContinuationOptions.AttachedToParent, taskScheduler);
            StartObservingTask(taskObserver, taskRunner, cancellationTokenSource);
            return taskRunner;
        }
        public TaskScheduler TaskScheduler => taskScheduler;
        public CancellationTokenSource CancellationTokenSource => CreateCancellationTokenSource();
        public bool CascadeCancelation { get => cascadeCancelation; set => cascadeCancelation = value; }

        public void UnsubscribeAll()
        {
            ITaskObserver[] subscriptions = this.Provider.Subscriptions;
            foreach (ITaskObserver subscription in subscriptions)
            {
                subscription.Unsubscribe();
            }
        }
        private IActionObserver<TAction> SubscribeActionJob<TAction>(TAction action)
        {
            IActionObserver<TAction> observer = GetSubscriber<TAction>();
            observer.SetJob(JobAction<TAction>.Create().Set(action)).Subscribe(Provider);
            return observer;
        }
        
        private IActionObserver<TAction> SubscribeActionJob<TAction, TArgs>(TAction action, TArgs[] args)
        {
            IActionObserver<TAction> observer = GetSubscriber<TAction>();
            observer.SetJob(JobActionArgs<TAction, TArgs>.Create().Set(action, args)).Subscribe(Provider);
            return observer;
        }
        
        private Task TaskObserverCallbacks(ITaskObserver taskObserver, Task<IJobRunner> taskRunner)
        {
            return taskRunner.ContinueWith((task, obj) =>{
                ITaskObserver o = (ITaskObserver)obj;
                RunCallbacks(o);
                
            },taskObserver, CancellationToken.None, TaskContinuationOptions.AttachedToParent, taskScheduler);
        }

        private void RunCallbacks(ITaskObserver observer)
        {
          
            bool observerError = observer.Status == ObserverStatus.CompletedWithErrors ||
                                 observer.Status == ObserverStatus.Canceled ||
                                 observer.Status == ObserverStatus.ExecutionTimeExceeded;
			try
			{
                if (observerError)
                    observer.OnError(new FifoTaskQueueWorkflowException("Error running Task. See Log. Status: " + observer.Status));
                else
                    observer.OnCompleted();
			}
			catch(Exception e)
			{
                observer.OnError(e);
                logger.Error(e);
            }            
        }

        private CancellationTokenSource CreateCancellationTokenSource()
        {
            if (cascadeCancelation)
			{
                if (cascadeCancellationTokenSource == null)
                {
                    cascadeCancellationTokenSource = new CancellationTokenSource();
                }
                return cascadeCancellationTokenSource;
			}
			else
			{
                return new CancellationTokenSource();
            }
        }
        private bool IsTaskObserverAtWork()
        {
            return Provider.Subscriptions.ToList().Exists(o => o.RunningTask != null);
        }
        private ITaskObserver GetLastObserver()
        {
            return Provider.Subscriptions.ToList().Last();
        }

        public ITasksProvider Provider => tasksProvider;

		public int JobMaximalExceutionTime {
            get => jobMaximalExecutionTime;
            set => jobMaximalExecutionTime = value; 
        }

		private IActionObserver<TAction> GetSubscriber<TAction>()
        {
            return TaskObserver<TAction>.Create(logger);
        }
        private void StartObservingTask(ITaskObserver taskObserver, Task<IJobRunner> jobRunner, CancellationTokenSource cancellationTokenSource)
		{
            taskObserver.OnNext(jobRunner);
            taskObserver.CancellationTokenSource = cancellationTokenSource;
            taskObserver.MaximumExecutionTime = JobMaximalExceutionTime;
        }
    }
}
