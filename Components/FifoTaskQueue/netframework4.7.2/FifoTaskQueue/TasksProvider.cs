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
using fmacias.Components.FifoTaskQueueAbstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    internal class TasksProvider : ITasksProvider
    {
        private List<ITaskObserver<Task>> observers;
        private readonly ILogger logger;
        private TasksProvider(ILogger logger)
        {
            observers = new List<ITaskObserver<Task>>();
            this.logger = logger;
        }
        public static TasksProvider Create(ILogger logger)
        {
            return new TasksProvider(logger);
        }
        public IDisposable Subscribe(IObserver<Task> observer)
        {
            if (!HasObserverBeenRegistered(observer))
                observers.Add((ITaskObserver<Task>)observer);
            return ObserverUnsubscriber<Task>.Create(observers, (ITaskObserver<Task>)observer);
        }
        public List<Task> Tasks => GetProcessingTasks();
        public void AddTask(Task task)
        {
            Tasks.Add(task);
        }
        public IObserver<Task> GetRequiredObserverByTask(Task task)
        {
            return observers.First<IObserver<Task>>(
                observer => Object.ReferenceEquals(((TaskObserver)observer).ObservableTask, task));
        }
        public bool ObserverSubscritionExist(Task task)
        {
            return observers.Exists(observer => Object.ReferenceEquals(((TaskObserver)observer).ObservableTask, task));
        }
        public bool ObserverSubscritionExist()
        {
            return observers.Count > 0;
        }
        public static bool HasTaskBeenFinished(Task task)
        {
            return (task.IsCompleted || task.IsCanceled || task.IsFaulted);
        }
        public List<ITaskObserver<Task>> Observers => observers;
        private bool HasObserverBeenRegistered(IObserver<Task> observer)
        {
            return observers.Contains(observer);
        }

        private List<Task> GetProcessingTasks()
        {
            List<Task> processingTasks = new List<Task>();
            observers.ForEach((observer) =>
            {
                if (!(observer.ObservableTask is null))
                    processingTasks.Add(observer.ObservableTask);
            });
            return processingTasks;
        }
    }
}
