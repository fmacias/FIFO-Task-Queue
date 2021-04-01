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
using System;
using System.Collections.Generic;
using System.Linq;
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
        const int MAX_QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS = 10000;
        private readonly TaskScheduler taskScheduler;
        private readonly TasksProvider tasksProvider;
        private CancellationTokenSource cancellationTokenSource;
        private bool excludeTaskCleanUpAfterFinalization = false;
        #region Constructor
        private FifoTaskQueue(TaskScheduler taskScheduler, TasksProvider tasksProvider)
        {
            this.taskScheduler = taskScheduler;
            this.tasksProvider = tasksProvider;
            this.tasksProvider.TaskFinishedEventHandler += HandleTaskFinished;
        }
        public static FifoTaskQueue Create(TaskScheduler taskSheduler, TasksProvider tasksProvider)
        {
            return new FifoTaskQueue(taskSheduler, tasksProvider);
        }
        #endregion
        #region private
        private void HandleTaskFinished(object sender, Task task)
        {
            Console.WriteLine(string.Format("Task {0} observation completed. Task Must be finished. Status:{1} ", task.Id, task.Status));
            UnsubscribeTaskObserver(task);
            if (object.ReferenceEquals(GetLastTask(), task))
            {
                Console.WriteLine("All Queued Tasks have already been finalized!");
                CleanCancelationToken();
            }
            if (!excludeTaskCleanUpAfterFinalization)
                RemoveDisposableTask(task);
        }

        private void RemoveDisposableTask(Task task)
        {
            if (IsTaskDisposable(task))
            {
                List<int> taskIds = new List<int>();
                taskIds.Add(task.Id);
                RemoveTasks(taskIds);
            }
        }

        private void UnsubscribeTaskObserver(Task task)
        {
            if (tasksProvider.ObserverSubscritionExist(task))
                ((TaskObserver)tasksProvider.GetRequiredObserverByTask(task)).Unsubscribe();
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
        private Action<Task, object> AssociateActionToTask(Action<object> action)
        {
            Action<Task, object> actionTask = (task, parameters) =>
            {
                action(parameters);
            };
            return actionTask;
        }
         void ObserveTask(Task task)
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
            TaskObserver observableTask = TaskObserver.Create(task).SetPollingStopElapsedTime(MAX_QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS);
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
            if (!AreTasksAvailable())
            {
                this.ObserveTask(Start(action));
            }
            else
            {
                this.ObserveTask(Continue(action));
            }
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
            if (!AreTasksAvailable())
            {
                this.ObserveTask(this.Start(action, parameters));
            }
            else
            {
                this.ObserveTask(Continue(action, parameters));
            }
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
        /// Manages the observation of tasks for these completation,  so that  
        /// not needed resources(Tasks, Observers and CancelaTionToken) are being 
        /// cleaned at execution time for a better performance.
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
        public async Task<bool> ObserveCompletation()
        {
            try
            {
                return await tasksProvider.ObserversCompletation();
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
            return true;
        }
        /// <summary>
        /// It is like <see cref="ObserveCompletation(int)"/> but excludes the clean up to Task after queue finalization
        /// to be able to check the task list of the queue after finalization.
        /// 
        /// A method <see cref="ClearUpTasks"/> is also provided.
        /// </summary>
        /// <param name="excludeTaskCleanUpAfterFinalization"></param>
        /// <param name="taskCancelationTime"></param>
        /// <returns></returns>
        public async Task<bool> ObserveCompletation(bool excludeTaskCleanUpAfterFinalization)
        {
            this.excludeTaskCleanUpAfterFinalization = excludeTaskCleanUpAfterFinalization;
            return await this.ObserveCompletation();

        }
        /// <summary>
        /// Planificate one Queue cancelation after the elapsed time given at taskCancelationTime if provided
        /// or after the default elapsed time givent at <see cref="MAX_QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS"/>.
        /// 
        /// Explanation
        /// -----------
        /// If the task takes longer, it will be abandoned. The observer will leave the obeservation but the task 
        /// wont be removed. Whenever it happens, you should provide to this task the queue CancelationToken to be able
        /// to cancel and bring those kind of long tasks to a default state.
        /// 
        /// </summary>
        /// <param name="taskCancelationTime"></param>
        /// <returns></returns>
        public async Task<bool> CancelAfter(int taskCancelationTime, bool excludeTaskCleanUpAfterFinalization = false)
        {
            taskCancelationTime = (taskCancelationTime > 0) ? taskCancelationTime : MAX_QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS;
            cancellationTokenSource.CancelAfter(taskCancelationTime);
            return await this.ObserveCompletation(excludeTaskCleanUpAfterFinalization);
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
                    Task<bool> completed = CancelAfter(MAX_QUEUE_CANCELATION_ELAPSED_TIME_MILISECONDS);
                    completed.Wait();
                }
                if (tasksProvider.ObserverSubscritionExist())
                {
                    throw new FifoTaskQueueDisposeException("Any Observer should be present after completation.");
                }
                this.ClearUpTasks();
                if (Tasks.Count() > 0)
                {
                    throw new FifoTaskQueueDisposeException("Any Task should be present after observer completation.");
                }
                this.cancellationTokenSource.Dispose();
                this.tasksProvider.TaskFinishedEventHandler -= HandleTaskFinished;
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