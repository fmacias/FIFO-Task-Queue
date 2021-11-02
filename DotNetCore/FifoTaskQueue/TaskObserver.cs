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
using System;
using System.Threading.Tasks;

namespace fmacias
{
    public class TaskObserver : IObserver<Task>
    {
        private readonly Task task;
        private IDisposable cancellation;
        private Task<bool> taskStatusCompletedTransition=Task.Run(()=> { return false; });
        private event EventHandler<TaskObserver> TaskFinishedEventHandler;
        private bool Completed = false;
        public Task ObservableTask => task;
        private TaskObserver(Task task)
        {
            this.task = task;
            TaskFinishedEventHandler += HandleTaskFinished;
        }
        public static TaskObserver Create(Task task)
        {
            return new TaskObserver(task);
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
            Console.WriteLine(string.Format("Task id: {0} Will be observe. State: {1}", task.Id, task.Status));
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
                Console.WriteLine(string.Format("Task id: {0} initial status {1}", task.Id, task.Status));
                while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted))
                {
                    if (currentStatus != task.Status)
                    {
                        Console.WriteLine(string.Format("Task id: {0} Status transition to {1}", task.Id, task.Status));
                        currentStatus = task.Status;
                    }
                }
                Console.WriteLine(string.Format("Task id: {0},  final status {1}, Duration: {2}", task.Id, task.Status, watch.ElapsedMilliseconds));
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
