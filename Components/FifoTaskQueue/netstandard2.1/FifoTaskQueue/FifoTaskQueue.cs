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
using System.Runtime.CompilerServices;
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
        const int QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS = 10000;
        private readonly TaskScheduler taskScheduler;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        private TasksProvider tasksProvider;
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
        #region private
        private TasksProvider Provider
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
            return (Tasks.Count > 0);
        }
        private Task GetLastTask()
        {
            return Tasks.Last();
        }
        private Action<Task> AssociateActionToTask(Action action)
        {
            Action<Task> actionTask = task =>
            {
                action();
            };
            return actionTask;
        }
        private static bool IsAsycn(Action<object> action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }
        private static bool IsAsycn(Action action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }

        private Action<Task, object> AssociateActionToTask(Action<object> action)
        {
            Action<Task, object> actionTask = (task, parameters) =>
            {
                action(parameters);
            };
            return actionTask;
        }
        private ITaskObserver<Task> ObserveTask(Task task)
        {
            ITaskObserver<Task> observer = (ITaskObserver<Task>)Provider.GetRequiredObserverByTask(task);
            Provider.GetRequiredObserverByTask(task).OnNext(task);
            return observer;
        }
        private CancellationToken CreateQueueCancelationToken()
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }
            return cancellationTokenSource.Token;
        }
        private void AddTask(Task task)
        {
            TaskObserver observableTask = TaskObserver.Create(task,logger);
            observableTask.Subscribe(Provider);
        }
        private Task Start(Action action)
        {
            Task task = Task.Factory.StartNew(action, CreateQueueCancelationToken(), TaskCreationOptions.None, taskScheduler);
            AddTask(task);
            return task;
        }
        private Task Start(Action<object> action, object paramters)
        {
            Task task = Task.Factory.StartNew(action, paramters, CreateQueueCancelationToken(), TaskCreationOptions.None, taskScheduler);
            AddTask(task);
            return task;
        }
        private Task Continue(Action action)
        {
            Task task = GetLastTask().ContinueWith(AssociateActionToTask(action), CreateQueueCancelationToken(), TaskContinuationOptions.None, taskScheduler);
            AddTask(task);
            return task;
        }
        private Task Continue(Action<object> action, object paramters)
        {
            Task task = GetLastTask().ContinueWith(AssociateActionToTask(action), paramters, CreateQueueCancelationToken(), TaskContinuationOptions.None, taskScheduler);
            AddTask(task);
            return task;
        }
        private void CleanCancelationToken()
        {
            if (Tasks.Count == 0)
            {
                this.cancellationTokenSource.Dispose();
            }
        }
        #endregion
        /// <summary>
        /// The Sheduler <see cref="T:System.Threading.Tasks.TaskScheduler" />
        /// <seealso cref="TaskShedulerWraper"/>
        /// </summary>
        public TaskScheduler TaskSheduler
        {
            get { return taskScheduler; }
        }
        /// <summary>
        /// Start Action without parameters and Returns the queue
        /// as a fluent interface.
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>ITaskQueue</returns>

        public ITaskObserver<Task> Run(Action action)
        {
            Task queuedTask;

            if (!AreTasksAvailable())
            {
                queuedTask = Start(action);
            }
            else
            {
                queuedTask = Continue(action);
            }
            RefuseAsync(action);
            Complete().Wait();
            return ObserveTask(queuedTask);
        }

        private void RefuseAsync(Action action)
        {
            if (IsAsycn(action))
                throw new FifoTaskQueueException("Async Methods do not make sense at the queue and are not allowed.");
        }
        private void RefuseAsync(Action<object> action)
        {
            if (IsAsycn(action))
                throw new FifoTaskQueueException("Asyc Methods do not make sense at the queue and are not allowed.");
        }
        /// <summary>
        /// Start Action with parameters and Returns the queue
        /// as a fluent interface.
        /// </summary>
        /// <param name="action"><![CDATA[Action<object>]]></param>
        /// <param name="parameters">object</param>
        /// <returns>ITaskQueue</returns>
        public ITaskObserver<Task> Run(Action<object> action, params object[] parameters)
        {
            Task queuedTask;
            if (!AreTasksAvailable())
            {
                queuedTask = this.Start(action, parameters);
            }
            else
            {
                queuedTask = Continue(action, parameters);
            }
            RefuseAsync(action);
            Complete().Wait();
            return ObserveTask(queuedTask);
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
        /// Observes the completation of the queue, it means, that the las task has been finished.
        /// 
        /// You can run this method whenever you want and so many times you want. 
        /// For example after each <see cref="Run(Action)"/> or 
        /// <see cref="Run(Action{object}, object)"/>, or after the ones that takes too long.
        /// 
        /// 
        /// Explanation:
        /// -------------------------------------------------------------------------------------------------
        /// Running or Planned Task are being managed by the <see cref="Task.Factory"/> as you can 
        /// see in the methods <see cref="Start(Action)"/>, <see cref="Start(Action{object}, object)"/>,
        /// <see cref="Continue(Action)"/> and <see cref="Continue(Action{object}, object)"/>. But 
        /// these status change is being observed pararell by the subcribers <see cref="TaskObserver"/> 
        /// in the <see cref="provider"/>, so that the queue is able to track itself that the execution 
        /// of tasks is being performed properly as expected. This logic can also be reused to create another 
        /// async. Queue, making the component more scalable.
        /// 
        /// This method can be invoked so many times you need. For example after definition 
        /// of each <see cref="Run(Action)"/> or <see cref="Run(Action{object}, object)"/> methods.
        /// 
        /// Tests provided at its UnitTest Class.
        /// <see cref="Run(Action)"/>
        /// </summary>
        /// <param name="taskCancelationTime"></param>
        /// <returns></returns>
        public async Task<bool> Complete()
        {
            try
            {
                List<bool> performedObservableTasks = new List<bool>();
                List<TaskObserver> completedTaskObservers = new List<TaskObserver>();
                foreach (IObserver<Task> observer in Provider.Observers)
                {
                    TaskObserver taskObserver = (TaskObserver)observer;
                    bool observerCompleted = await taskObserver.TaskStatusCompletedTransition;
                    performedObservableTasks.Add(observerCompleted);
                    completedTaskObservers.Add(taskObserver);
                    string success = observerCompleted ? "successfully" : "unsuccessfully";
                    logger.Debug(String.Format("Task {0} observation completed {1}", taskObserver.ObservableTask.Id, success));
                }
                return !(Array.IndexOf(performedObservableTasks.ToArray(), false) > -1);
            }
            catch (TaskCanceledException)
            {
                logger.Debug("\nTasks cancelled: timed out.\n");
            }
            catch (AggregateException ae)
            {
                TaskCanceledException exception = ae.InnerException as TaskCanceledException ?? throw ae;
                logger.Debug(string.Format("Task {0} Canceled.", exception.Task.Id));
            }
            return true;
        }
        /// <summary>
        /// Planificate the Queue cancelation after the elapsed time given at taskCancelationTime if provided
        /// or after the default elapsed time givent at <see cref="QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS"/>.
        /// 
        /// Explanation
        /// -----------
        /// If the task takes longer, it will be abandoned. The observer will leave the obeservation of the task 
        /// , but wont be removed. Whenever it happens, you should provide to this task the queue CancelationToken to be able
        /// carry those kind of long tasks to a canceled or to a faulted status.
        /// 
        /// </summary>
        /// <param name="tasksCancelationTime"></param>
        /// <returns></returns>
        public async Task<bool> CancelAfter(int tasksCancelationTime)
        {
            tasksCancelationTime = (tasksCancelationTime > 0) ? tasksCancelationTime : QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS;
            cancellationTokenSource.CancelAfter(tasksCancelationTime);
            return await Complete();
        }
        /// <summary>
        /// CancelationToken<see cref="CancellationToken"/> used to manage a cascade cancelation of running or planned tasks.
        /// Tests provided at its UnitTest Class.
        /// </summary>
        public CancellationToken CancellationToken => CreateQueueCancelationToken();
        /// <summary>
        /// Task to run provided by <see cref="provider"/>
        /// </summary>
        public List<Task> Tasks => Provider.Tasks;
        /// <summary>
        /// Disposes and Removes finished and non subscribed Task from the list.
        /// </summary>
        public void ClearUpTasks()
        {
            List<int> disposedTaskIds = new List<int>();
            foreach (Task task in Tasks)
            {
                if (IsTaskDisposable(task))
                {
                    disposedTaskIds.Add(task.Id);
                    task.Dispose();
                }
            }
            RemoveTasks(disposedTaskIds);
        }

        private void RemoveTasks(List<int> disposedTaskIds)
        {
            Tasks.RemoveAll(currentTask => Array.IndexOf(disposedTaskIds.ToArray(), currentTask.Id) > -1);
        }

        private bool IsTaskDisposable(Task task)
        {
            return (!Provider.ObserverSubscritionExist(task) && TasksProvider.HasTaskBeenFinished(task));
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (Provider.ObserverSubscritionExist())
                {
                    Task<bool> completed = Complete();
                    completed.Wait();
                    UnsubscribeObservers().Wait();
                }

                if (Provider.ObserverSubscritionExist())
                {
                    throw new FifoTaskQueueException("Any Observer should be present after completation.");
                }

                ClearUpTasks();

                if (Tasks.Count() > 0)
                {
                    throw new FifoTaskQueueException("Any Task should be present after observer completation.");
                }
                this.cancellationTokenSource?.Dispose();
            }
        }
        private async Task<bool> UnsubscribeObservers()
        {
            List<TaskObserver> completedTaskObservers = new List<TaskObserver>();
            foreach (IObserver<Task> observer in Provider.Observers)
            {
                TaskObserver taskObserver = (TaskObserver)observer;
                bool observerCompleted = await taskObserver.TaskStatusCompletedTransition;
                completedTaskObservers.Add(taskObserver);
            }
            completedTaskObservers.ForEach(taskObserver =>
            {
                taskObserver.Unsubscribe();
                logger.Debug(String.Format("Observer of Task {0} unsubscribed!", taskObserver.ObservableTask.Id));
            });
            return true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<ITaskObserver<Task>> Process(Action<object> action, params object[] parameters)
        {
            ITaskObserver<Task> observer = this.Run(action, parameters);
            await Complete();
            return observer;
        }

        public async Task<ITaskObserver<Task>> Process(Action action)
        {
            ITaskObserver<Task> observer = this.Run(action);
            await Complete();
            return observer;
        }

        ~FifoTaskQueue()
        {
            Dispose(false);
        }
    }
}