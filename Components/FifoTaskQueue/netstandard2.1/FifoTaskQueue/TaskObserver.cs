/**
 * LICENSE
 *
 * This source file is subject to the new BSD license that is bundled
 * with this package in the file LICENSE.txt.
 *
 * @copyright   Copyright (c) 2021. Fernando Macias Ruano.
 * @E-Mail      fmaciasruano@gmail.com > .
 * @license    https://github.com/fmacias/Scheduler/blob/master/Licence.txt
 */
using NLog;
using System;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    internal class TaskObserver : IObserver<Task>
    {
        private const int MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS = 10000;
        private readonly Task task;
        private readonly ILogger logger;

        private IDisposable cancellation;
        private Task<bool> taskStatusCompletedTransition=Task.Run(()=> { return false; });
        private event EventHandler<TaskObserver> TaskFinishedEventHandler;
        private bool Completed = false;
        public Task ObservableTask => task;

        private TaskObserver(Task task,ILogger logger)
        {
            this.task = task;
            TaskFinishedEventHandler += HandleTaskFinished;
            this.logger = logger;
        }
        public static TaskObserver Create(Task task, ILogger logger)
        {
            return new TaskObserver(task,logger);
        }
        public Task<bool> TaskStatusCompletedTransition => taskStatusCompletedTransition;
        public void OnCompleted()
        {
            Completed = true;
        }
        public virtual void Subscribe(TasksProvider provider)
        {
            cancellation = provider.Subscribe(this);
        }
        public virtual void Unsubscribe()
        {
            cancellation.Dispose();
            TaskFinishedEventHandler -= HandleTaskFinished;
        }
        public void OnError(Exception error)
        {
            Console.Write(error.ToString());
        }
        public void OnNext(Task value)
        {
            if (!Object.ReferenceEquals(task, value))
            {
                return;
            }
            logger.Debug(string.Format("Task id: {0} Will be observe. State: {1}", task.Id, task.Status));
            PollingTaskStatusTransition();
        }
        private void PollingTaskStatusTransition()
        {
            taskStatusCompletedTransition.Wait();
            taskStatusCompletedTransition.Dispose();
            taskStatusCompletedTransition = Task.Run(()=> 
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                TaskStatus currentStatus = task.Status;
                logger.Debug(string.Format("Task id: {0} initial status {1}", task.Id, task.Status));

                while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted) && (watch.ElapsedMilliseconds <= MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS))
                {
                    if (currentStatus != task.Status)
                    {
                        logger.Debug(string.Format("Task id: {0} Status transition to {1}", task.Id, task.Status));
                        currentStatus = task.Status;
                    }
                }
                logger.Debug(string.Format("Task id: {0},  final status {1}, Duration: {2}", task.Id, task.Status, watch.ElapsedMilliseconds));
                watch.Stop();
                OnTaskFinished();
                return true;
            }); 
        }
        protected virtual void OnTaskFinished()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<TaskObserver> raiseEvent = TaskFinishedEventHandler;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Call to raise the event.
                raiseEvent(this, this);
            }
        }
        private void HandleTaskFinished(object sender, TaskObserver oberver)
        {
            oberver.OnCompleted();
        }
    }
}
