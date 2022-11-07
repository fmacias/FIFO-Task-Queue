using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FifoTaskQueueAbstract;

namespace FifoTaskQueue2
{
    public class FifoTaskQueue: ITaskQueue
    {
        private readonly TaskScheduler taskSheduller;
        private readonly ITasksProvider tasksProvider;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;

        #region Constructor

        protected FifoTaskQueue(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger)
        {
            this.taskSheduller = taskScheduler;
            this.tasksProvider = provider;
            this.logger = logger;
        }
        public static FifoTaskQueue Create(TaskScheduler taskScheduler, ITasksProvider provider, ILogger logger)
        {
            return new FifoTaskQueue(taskScheduler, provider, logger);
        }
        private void asdf()
        {
            Define<Action>(() => { }).OnCompleteCallback((object sender) => { });
            Define<Action<int>>((arg) => { }, 1).OnCompleteCallback((object sender) => { });
        }

        #endregion
        public IActionObjserverNoParams<TAction> Define<TAction>(TAction action)
        {
            return SubscribeActionJob(action);
        }
        public IActionObserver<TAction, object> Define<TAction>(TAction action, params object[] args)
        {
            return SubscribeActionJob(action, args);
        }
        public async Task<ITaskQueue> Perform()
        {
            List<IObserver<Task>> subscriptionsCopy = tasksProvider.Subscriptions.ToList();
            subscriptionsCopy.ForEach(async s =>
            {
                ITaskObserver taskObserver = s as ITaskObserver;
                IJobRunner jobRunner = taskObserver.Runner;
                try
                {
                    await StartOrContinueTask(jobRunner, taskObserver);
                }
                catch (Exception e)
                {
                    taskObserver.OnError(e);
                }
                finally
                {
                    taskObserver.PublishEvents();
                    taskObserver.Unsubscribe();
                }
            });
            return this;
        }
        public async Task<ITaskQueue> CancelAfter(int taskCancelationTime)
        {
            cancellationTokenSource.CancelAfter(taskCancelationTime);
            await Perform();
            return this;
        }
        public void CancelExecution()
        {
            cancellationTokenSource.Cancel();
        }
        public TaskScheduler TaskScheduler => taskSheduller;

        public CancellationToken CancellationToken => GetQueueCancelationToken();

        public IEnumerable<Task> Tasks => tasksProvider.GetProcessingTasks();

        private IActionObjserverNoParams<TAction> SubscribeActionJob<TAction>(TAction action)
        {
            IActionObjserverNoParams<TAction> observer = GetSubscriber<TAction>();
            observer.SetJob(JobAction<TAction, object>.Create().Set(action)).Subscribe(Provider);
            return observer;
        }
        private IActionObserver<TAction, TArgs> SubscribeActionJob<TAction, TArgs>(TAction action, TArgs[] args)
        {
            IActionObserver<TAction, TArgs> observer = GetSubscriber<TAction, TArgs>();
            observer.SetJob(JobAction<TAction, TArgs>.Create().Set(action, args)).Subscribe(Provider);
            return observer;
        }
        private async Task StartOrContinueTask(IJobRunner runner, ITaskObserver taskObserver)
        {
            Task task;

            if (!AreTasksAvailable())
                task = Start(runner);
            else
                task = Continue(runner);
            taskObserver.OnNext(task);
            Task<bool> completedTransition = taskObserver.TaskStatusCompletedTransition;
            await task;
            await completedTransition;
        }
        private bool AreTasksAvailable()
        {
            return Tasks.ToList().Count > 0;
        }
        private Task Start(IJobRunner runner)
        {
            return Task.Factory.StartNew(
                runner.StartAction,
                runner,
                this.CancellationToken, 
                TaskCreationOptions.None, 
                this.TaskScheduler);
        }
        private Task Continue(IJobRunner runner)
        {
            return this.Tasks.Last().ContinueWith(
                runner.ContinueAction,
                runner,
                CancellationToken,
                TaskContinuationOptions.None,
                this.TaskScheduler);
        }
        private CancellationToken GetQueueCancelationToken()
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }
            return cancellationTokenSource.Token;
        }
        private ITasksProvider Provider
        {
            get
            {
                return tasksProvider;
            }
        }
        private IActionObserver<TAction, TArgs> GetSubscriber<TAction, TArgs>()
        {
            return tasksProvider.SubscriberFactory.Create<TAction, TArgs>(logger);
        }
        private IActionObserver<TAction,object> GetSubscriber<TAction>()
        {
            return tasksProvider.SubscriberFactory.Create<TAction, object>(logger);
        }
    }
}
