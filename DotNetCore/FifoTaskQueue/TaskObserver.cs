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

namespace fmacias
{
    public class TaskObserver : IObserver<Task>
    {
        private readonly Task task;
        private readonly ILogger logger;

        private IDisposable cancellation;
        private Task<bool> taskStatusCompletedTransition=Task.Run(()=> { return false; });
        private TaskObserver(Task task,ILogger logger)
        {
            this.task = task;
            this.logger = logger;
        }
        public static TaskObserver Create(Task task, ILogger logger)
        {
            return new TaskObserver(task,logger);
        }
        public Task<bool> TaskStatusCompletedTransition => taskStatusCompletedTransition;
        public async void OnCompleted()
        {
            if (!(await taskStatusCompletedTransition))
                OnError(new Exception(string.Format("Task {0} Was not Completed at this point of execution!")));
            taskStatusCompletedTransition.Dispose();
            if (TasksProvider.HasTaskBeenFinished(task))
                task.Dispose();
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
                while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted))
                {
                    if (currentStatus != task.Status)
                    {
                        logger.Debug(string.Format("Task id: {0} Status transition to {1}", task.Id, task.Status));
                        currentStatus = task.Status;
                    }
                }
                logger.Debug(string.Format("Task id: {0},  final status {1}, Duration: {2}", task.Id, task.Status, watch.ElapsedMilliseconds));
                watch.Stop();
                return true;
            });
        }
        public Task ObservableTask => task;
    }
}
