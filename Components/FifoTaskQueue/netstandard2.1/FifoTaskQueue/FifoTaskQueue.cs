/**
 * LICENSE
 *
 * This source file is subject to the new BSD license that is bundled
 * with this package in the file LICENSE.txt.
 *
 * @copyright   Copyright (c) 2021. Fernando Macias Ruano.
 * @E-Mail      fmaciasruano@gmail.com .
 * @license    https://github.com/fmacias/Scheduler/blob/master/Licence.txt
 */
using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    /// <summary>
    /// Defines a group of <see cref="System.Threading.Tasks.Task" /> to be excecuted at a given 
    /// <see cref="T:System.Threading.Tasks.TaskScheduler" /> according to the FIFO(First Input 
    /// first output) concept.
    /// </summary>
    public class FifoTaskQueue : ITaskQueue
    {
        private readonly TaskScheduler taskScheduler;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        private ITasksProvider tasksProvider;

        #region Constructor

        protected FifoTaskQueue(TaskScheduler taskScheduler, ILogger logger)
        {
            this.taskScheduler = taskScheduler;
            this.logger = logger;
        }

        public static FifoTaskQueue Create(TaskScheduler taskSheduler, ILogger logger)
        {
            return new FifoTaskQueue(taskSheduler, logger);
        }

        #endregion
        #region Interface Implementation

        /// <summary>
        /// Usage: 
        ///     queue.Define<Action>([Action])
        ///     queue.Define<Action<object>>([Action<object>])
        /// </summary>
        /// <typeparam name="TAction"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public IActionObserver<TAction> Define<TAction>(TAction action)
        {
            return SubscribeObserver<TAction>().SetAction(action);
        }

        public ITaskQueue Run<TAction>(IActionObserver<TAction> observer)
        {
            RunActionAtTask<TAction,object>(observer);
            return this;
        }
        public ITaskQueue Run<TAction,TArgs>(IActionObserver<TAction> observer, params TArgs[] args)
        {
            RunActionAtTask<TAction, TArgs>(observer,args);
            return this;
        }

        private ITaskQueue RunActionAtTask<TAction,TArgs>(IActionObserver<TAction> observer, TArgs[] args =null)
        {
            Task queuedTask;

            if (!AreTasksAvailable())
                queuedTask = (args is null) ? Start(observer) : Start<TAction,TArgs>(observer, args);
            else
                queuedTask = (args is null) ? Continue(observer) : Continue<TAction,TArgs>(observer, args);

            observer.OnNext(queuedTask);
            return this;
        }

        /// <summary>
        /// Awaitable method to await processing the queue whenever is required at async methods.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Complete()
        {
            List<bool> observers = await Provider.CompleteQueueObservation();
            return !HaveObserversBeenPerformed(observers);
        }

        /// <summary>
        /// Cancel Queue after given elapsed time.
        /// </summary>
        /// <param name="tasksCancelationTime"></param>
        /// <returns></returns>
        public async Task<bool> CancelAfter(int tasksCancelationTime)
        {
            cancellationTokenSource.CancelAfter(tasksCancelationTime);
            return await Complete();
        }

        /// <summary>
        /// Forces queue cancelation of tasks
        /// Unit Test are provided at its Test Class.
        /// </summary>
        public void CancelExecution()
        {
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch { }
        }

        /// <summary>
        /// The Sheduler <see cref="T:System.Threading.Tasks.TaskScheduler" />
        /// <seealso cref="TaskShedulerWraper"/>
        /// </summary>
        public TaskScheduler TaskSheduler
        {
            get { return taskScheduler; }
        }

        /// <summary>
        /// CancelationToken<see cref="CancellationToken"/> used to manage a cascade cancelation of running or planned tasks.
        /// Tests provided at its UnitTest Class.
        /// </summary>
        public CancellationToken CancellationToken => GetQueueCancelationToken();

        /// <summary>
        /// Task to run provided by <see cref="provider"/>
        /// </summary>
        public List<Task> Tasks => Provider.GetProcessingTasks();

        #endregion

        #region private
        private static bool HaveObserversBeenPerformed(List<bool> performedObservableTasks)
        {
            return (Array.IndexOf(performedObservableTasks.ToArray(), false) > -1);
        }
        private TaskObserver<TAction> SubscribeObserver<TAction>()
        {
            var observableTask = TaskObserver<TAction>.Create(logger);
            observableTask.Subscribe(Provider);
            return observableTask;
        }

        private ITasksProvider Provider
        {
            get
            {
                if (tasksProvider == null)
                    tasksProvider = TasksProvider.Create(logger);

                return tasksProvider;
            }
        }

        private bool AreTasksAvailable()
        {
            return Tasks.Count > 0 || Tasks is null;
        }

        private Task GetLastTask()
        {
            return Tasks.Last();
        }

        private Action<Task> ActionTaskNoParams<TAction>(IActionObserver<TAction> actionObserver)
        {
            Action<Task> actionTask = task =>
            {
                ExecutorWithParams<TAction>.Create(actionObserver).Execute();
            };
            return actionTask;
        }

        private Action<Task,object> ActionTaskParams<TAction,TArgs>()
        {
            Action<Task,object> actionTask = (task, args) =>
            {
                ExecutorWithParams<TAction, TArgs> queuRun = args as ExecutorWithParams<TAction, TArgs>;
                queuRun.Execute();
            };
            return actionTask;
        }
        private Action<object> ActionParams<TAction,TArgs>()
        {
            Action<object> ao = (args)=> {
                ExecutorWithParams<TAction, TArgs> queuRun = args as ExecutorWithParams<TAction, TArgs>;
                queuRun.Execute();
            };
            return ao;
        }

        private CancellationToken GetQueueCancelationToken()
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }
            return cancellationTokenSource.Token;
        }

        private Task Start<TAction>(IActionObserver<TAction> observer)
        {
            return Task.Factory.StartNew(
                observer.GetAction() as Action,
                GetQueueCancelationToken(),
                TaskCreationOptions.None,
                taskScheduler);
        }

        private Task Start<TAction,TArgs>(IActionObserver<TAction> observer, TArgs[] args)
        {
            return Task.Factory.StartNew(
                ActionParams<TAction, TArgs>(),
                ExecutorWithParams<TAction, TArgs>.Create(observer, args),
                GetQueueCancelationToken(), TaskCreationOptions.None, taskScheduler);
        }
        private Task Continue<TAction>(IActionObserver<TAction> observer)
        {
            return GetLastTask().ContinueWith(
                ActionTaskNoParams(observer),
                GetQueueCancelationToken(),
                TaskContinuationOptions.None,
                taskScheduler);
        }

        private Task Continue<TAction,TArgs>(IActionObserver<TAction> observer, TArgs[] args)
        {
            return GetLastTask().ContinueWith(
                ActionTaskParams<TAction, TArgs>(),
                ExecutorWithParams<TAction, TArgs>.Create(observer, args),
                GetQueueCancelationToken(),
                TaskContinuationOptions.None,
                taskScheduler);
        }
        #endregion

        #region Disposable Pattern
        
        protected virtual void Dispose(bool disposing)
        {

            if (disposing)
            {
                Provider.UnsubscribeObservers().Wait();

                if (Tasks.Count() > 0)
                {
                    throw new FifoTaskQueueException("Any Task should be present after observer completation.");
                }
                this.cancellationTokenSource?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Managed by the Garbage Collector
        /// </summary>
        ~FifoTaskQueue()
        {
            Dispose(false);
        }

        #endregion
    }
}
