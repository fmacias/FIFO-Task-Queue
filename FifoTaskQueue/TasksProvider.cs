﻿/**
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace fmacias
{
    public class TasksProvider : IObservable<Task>
    {
        private List<IObserver<Task>> observers;
        private readonly List<Task> tasks;
        public event EventHandler<Task> TaskFinishedEventHandler;
        private TasksProvider(List<Task> tasks)
        {
            observers = new List<IObserver<Task>>();
            this.tasks = tasks;
        }
        public static TasksProvider Create(List<Task> tasks)
        {
            return new TasksProvider(tasks);
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
            IObserver<Task> observerTask;
            try
            {
                observerTask = observers.First<IObserver<Task>>(
                observer => Object.ReferenceEquals(((TaskObserver)observer).ObservableTask, task));
            }catch(System.InvalidOperationException)
            {
                Console.WriteLine(string.Format("Task {0} was not registered",task.Id));
                observerTask = TaskObserver.Create(task);
                ((TaskObserver)observerTask).Subscribe(this);
            }
            return observerTask;
        }
        public bool ObserverSubscritionExist(Task task)
        {
            return observers.Exists(observer => Object.ReferenceEquals(((TaskObserver)observer).ObservableTask, task));
        }
        public bool ObserverSubscritionExist()
        {
            return observers.Count > 0;
        }
        public async Task<bool> ObserversCompletation()
        {
            List<Task> potentiallyFinishedTasks = new List<Task>();
            foreach (IObserver<Task> observer in observers)
            {
                TaskObserver taskObserver = (TaskObserver)observer;
                await taskObserver.TaskStatusCompletedTransition;
                potentiallyFinishedTasks.Add(taskObserver.ObservableTask);
                taskObserver.OnCompleted();
            }
            foreach (Task task in potentiallyFinishedTasks)
            {
                OnTaskFinished(task);
            }
            return true;
        }
        protected virtual void OnTaskFinished(Task task)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Task> raiseEvent = TaskFinishedEventHandler;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Call to raise the event.
                raiseEvent(this, task);
            }
        }
        private bool HasObserverBeenRegistered(IObserver<Task> observer)
        {
            return observers.Contains(observer);
        }
        public static bool HasTaskBeenFinished(Task task)
        {
            return (task.IsCompleted || task.IsCanceled || task.IsFaulted);
        }
    }
}
