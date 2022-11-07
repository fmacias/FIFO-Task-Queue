using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EventAggregatorAbstract.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;
using NLog.Targets.Wrappers;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public class FifoTaskQueue: ITaskQueue
    {
        private readonly TaskScheduler taskScheduler;
        private readonly ITasksProvider tasksProvider;
        private readonly ILogger logger;
        private readonly IEventAggregator eventAggregator;
        private readonly CancellationTokenSource cancellationTokenSource;
        private IProcessEventSubscriptor onQueueFinished;

        #region Constructor

        protected FifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger, IEventAggregator eventAggregator)
        {
            this.taskScheduler = taskScheduler;
            this.tasksProvider = provider;
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.onQueueFinished = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            SubscriptorHelper.AddUnicEventHandler((object sender) => { }, onQueueFinished, eventAggregator, this);
            cancellationTokenSource = new CancellationTokenSource();
        }
        
        public static FifoTaskQueue Create(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger,
        IEventAggregator eventAggregator)
        {
            return new FifoTaskQueue(taskScheduler, provider, logger,eventAggregator);
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
        
        public async Task<IJobRunner> Complete(params ITaskObserver[] observers)
        {
            ITaskObserver[] subscriptions = Provider.Subscriptions;
            int numOfSubscriptions = subscriptions.Length;

            Task<IJobRunner> previousRunner = this.Run(subscriptions[0]);
            Task<IJobRunner> currentRunner = previousRunner;
            await previousRunner;
            
            for (var x = 1; x < numOfSubscriptions; x++)
            {
                currentRunner = Continue(previousRunner, subscriptions[x]);
                await currentRunner;
                previousRunner = currentRunner;
            }

            return await currentRunner;
        }

        public async Task<ITaskQueue> CancelAfter(int miliseconds)
        {
            cancellationTokenSource.CancelAfter(miliseconds);
            await Complete();
            return this;
        }
        
        public void CancelExecution()
        {
            cancellationTokenSource.Cancel();
        }

        public Task<IJobRunner> Run(ITaskObserver taskObserver)
        {
           
            Task<IJobRunner> t = Task<IJobRunner>.Factory.StartNew((obj) =>
            {
                ITaskObserver observer = (ITaskObserver)obj;

                if (observer.Runner.IsAsync())
                {
                    Task<IJobRunner> runner = Task.Run<Task<IJobRunner>>(async () =>
                    {
                        return await Task.Run(() =>
                        {
                            return taskObserver.Runner.Run();
                        }, this.CancellationToken);
                    }, this.CancellationToken).Unwrap();
                    runner.Wait();
                    return runner.Result;
                } else
                {
                    return observer.Runner.Run();
                }
            }, taskObserver, CancellationToken, TaskCreationOptions.None, taskScheduler);
            taskObserver.OnNext(t);
            TaskObserverCallbacks(taskObserver, t);
            return t;
        }
        public Task<IJobRunner> Continue(Task<IJobRunner> previousTask, ITaskObserver taskObserver)
        {
            return previousTask.ContinueWith(
                (task, obj) =>
                {
                    ITaskObserver observer = (ITaskObserver)obj;
                    return this.Run(observer);
                },
                taskObserver,
                CancellationToken, TaskContinuationOptions.AttachedToParent, taskScheduler).Unwrap();
        }
        public TaskScheduler TaskScheduler => taskScheduler;

        public CancellationToken CancellationToken => GetQueueCancelationToken();
        public ITaskQueue OnQueueFinishedCallback(IProcessEvent.ProcessEventHandler handler)
        {
            this.onQueueFinished.Unsubscribe();
            this.onQueueFinished = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            SubscriptorHelper.AddUnicEventHandler(handler, onQueueFinished, eventAggregator, this);
            return this;
        }
        private static void Unsubscribe(ITaskQueue o)
        {
            ITaskObserver[] subscriptions = o.Provider.Subscriptions;
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
        
        private Task<IJobRunner> TaskObserverCallbacks(ITaskObserver taskObserver, Task<IJobRunner> taskRunner)
        {
            return taskRunner.ContinueWith(
                (task, obj) =>
                {
                    ITaskObserver o = (ITaskObserver)obj;
                    
                    if (o.Runner.IsAsync())
                        RunAsyncCallbacksSequentially(taskObserver, task);
                    else
                        RunCallbacks(o);

                    return task;
                },
                taskObserver,
                CancellationToken, TaskContinuationOptions.AttachedToParent, taskScheduler).Unwrap();
        }

        private static void RunCallbacks(ITaskObserver observer)
        {
           
            bool observerError = observer.Status == ObserverStatus.CompletedWithErrors ||
                                 observer.Status == ObserverStatus.Canceled ||
                                 observer.Status == ObserverStatus.ExecutionTimeExceeded;
            if (observerError)
                observer.OnError(new FifoTaskQueueWorkflowException("Error running Task. See Log. Status: " + observer.Status));
            else
                observer.OnCompleted();

            observer.Unsubscribe();
        }

        private void RunAsyncCallbacksSequentially(ITaskObserver taskObserver, Task<IJobRunner> taskRunner)
        {
            taskRunner.ContinueWith((task, obj) =>
            {
                ITaskObserver o = (ITaskObserver)obj;
                RunCallbacks(o);
                return task;
            }, taskObserver, CancellationToken, TaskContinuationOptions.AttachedToParent, taskScheduler).Unwrap().Wait();
        }

        private CancellationToken GetQueueCancelationToken()
        {
            return cancellationTokenSource.Token;
        }

        public ITasksProvider Provider => tasksProvider;

        private IActionObserver<TAction> GetSubscriber<TAction>()
        {
            return TaskObserver<TAction>.Create(eventAggregator, logger);
        }

        public void OnQueueFinished()
        {
            this.onQueueFinished.Publish();
        }
    }
}
