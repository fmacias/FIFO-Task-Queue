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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fmacias
{
    public class TasksProvider : IObservable<Task>
    {
        private List<IObserver<Task>> observers;
        private readonly List<Task> tasks;
        private readonly ILogger logger;
        private TasksProvider(List<Task> tasks, ILogger logger)
        {
            observers = new List<IObserver<Task>>();
            this.tasks = tasks;
            this.logger = logger;
        }
        public static TasksProvider Create(List<Task> tasks, ILogger logger)
        {
            return new TasksProvider(tasks,logger);
        }
        public IDisposable Subscribe(IObserver<Task> observer)
        {
            if (!HasObserverBeenRegistered(observer))
            {
                observers.Add(observer);
            }
            tasks.Add(((TaskObserver)observer).ObservableTask);
            return ObserverUnsubscriber<Task>.Create(observers, observer);
        }
        public List<Task> Tasks => tasks;
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
        private bool HasObserverBeenRegistered(IObserver<Task> observer)
        {
            return observers.Contains(observer);
        }
        public static bool HasTaskBeenFinished(Task task)
        {
            return (task.IsCompleted || task.IsCanceled || task.IsFaulted);
        }
        public List<IObserver<Task>> Observers => observers;
    }
}
