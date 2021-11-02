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
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace fmacias
{
    /// <summary>
    /// Defines a group of <see cref="System.Threading.Tasks.Task" /> to be excecuted at a given 
    /// <see cref="T:System.Threading.Tasks.TaskScheduler" /> according to the FIFO(First Input 
    /// first output) concept.
    /// </summary>
    public class FifoTaskQueue : ITaskQueue,IDisposable
    {
        const int QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS = 10000;
        private readonly TaskScheduler taskScheduler;
        private readonly TasksProvider tasksProvider;
        private readonly ILogger logger;
        private CancellationTokenSource cancellationTokenSource;
        #region Constructor
        private FifoTaskQueue(TaskScheduler taskScheduler, TasksProvider tasksProvider, ILogger logger)
        {
            this.taskScheduler = taskScheduler;
            this.tasksProvider = tasksProvider;
            this.logger = logger;
        }
        public static FifoTaskQueue Create(TaskScheduler taskSheduler, TasksProvider tasksProvider, ILogger logger)
        {
            return new FifoTaskQueue(taskSheduler, tasksProvider, logger);
        }
        #endregion
        #region private
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
                try
                {
                    action();
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("\nTasks cancelled: timed out.\n");
                }
                catch (AggregateException ae)
                {
                    TaskCanceledException exception = ae.InnerException as TaskCanceledException ?? throw ae;
                    Console.WriteLine(string.Format("Task {0} Canceled.", exception.Task.Id));
                }
            };
            return actionTask;
        }
        private Action<Task, object> AssociateActionToTask(Action<object> action)
        {
            Action<Task, object> actionTask = (task, parameters) =>
            {
                try
                {
                    action(parameters);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("\nTasks cancelled: timed out.\n");
                }
                catch (AggregateException ae)
                {
                    TaskCanceledException exception = ae.InnerException as TaskCanceledException ?? throw ae;
                    Console.WriteLine(string.Format("Task {0} Canceled.", exception.Task.Id));
                }
            };
            return actionTask;
        }
        private void ObserveTask(Task task)
        {
            tasksProvider.GetRequiredObserverByTask(task).OnNext(task);
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
            observableTask.Subscribe(tasksProvider);
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

        public ITaskQueue Run(Action action)
        {
            Task runningTask;
            if (!AreTasksAvailable())
            {
                runningTask = Start(action);
            }
            else
            {
                runningTask = Continue(action);
            }
            ObserveTask(runningTask);
            return this;
        }
        /// <summary>
        /// Start Action with parameters and Returns the queue
        /// as a fluent interface.
        /// </summary>
        /// <param name="action"><![CDATA[Action<object>]]></param>
        /// <param name="parameters">object</param>
        /// <returns>ITaskQueue</returns>
        public ITaskQueue Run(Action<object> action, object parameters)
        {
            Task runningTask;
            if (!AreTasksAvailable())
            {
                runningTask = this.Start(action, parameters);
            }
            else
            {
                runningTask = Continue(action, parameters);
            }
            ObserveTask(runningTask);
            return this;
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
        /// in the <see cref="TasksProvider"/>, so that the queue is able to track itself that the execution 
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
                foreach (IObserver<Task> observer in this.tasksProvider.Observers)
                {
                    TaskObserver taskObserver = (TaskObserver)observer;
                    bool observerCompleted = await taskObserver.TaskStatusCompletedTransition;
                    performedObservableTasks.Add(observerCompleted);
                    completedTaskObservers.Add(taskObserver);
                    string success = observerCompleted ? "successfully" : "unsuccessfully";
                    logger.Debug(String.Format("Task {0} observation completed {1}", taskObserver.ObservableTask.Id, success));
                }
                completedTaskObservers.ForEach(taskObserver =>
                {
                    taskObserver.Unsubscribe();
                    logger.Debug(String.Format("Observer of Task {0} unsubscribed!", taskObserver.ObservableTask.Id));
                });
                return (Array.IndexOf(performedObservableTasks.ToArray(), false) > -1);
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
        /// Task to run provided by <see cref="TasksProvider"/>
        /// </summary>
        public List<Task> Tasks => tasksProvider.Tasks;
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
            return (!tasksProvider.ObserverSubscritionExist(task) && TasksProvider.HasTaskBeenFinished(task));
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (tasksProvider.ObserverSubscritionExist())
                {
                    Task<bool> completed = Complete();
                    completed.Wait();
                }

                if (tasksProvider.ObserverSubscritionExist())
                {
                    throw new FifoTaskQueueException("Any Observer should be present after completation.");
                }

                ClearUpTasks();

                if (Tasks.Count() > 0)
                {
                    throw new FifoTaskQueueException("Any Task should be present after observer completation.");
                }
                this.cancellationTokenSource.Dispose();
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~FifoTaskQueue()
        {
            Dispose(false);
        }
    }
}