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
        private List<IObserver<Task>> observers;
        private readonly ILogger logger;
        private TasksProvider(ILogger logger)
        {
            observers = new List<IObserver<Task>>();
            this.logger = logger;
        }
        public static TasksProvider Create(ILogger logger)
        {
            return new TasksProvider(logger);
        }
        public IDisposable Subscribe(IObserver<Task> observer)
        {
            if (!HasObserverBeenRegistered(observer))
                observers.Add(observer);
            return ObserverUnsubscriber<Task>.Create(observers, observer);
        }
        public IObserver<Task> GetRequiredObserverByTask(Task task)
        {
            return observers.First(
                observer => Object.ReferenceEquals(((IObserver)observer).ObservableTask, task));
        }
        public bool ObserverSubscritionExist(Task task)
        {
            return observers.Exists(observer => ReferenceEquals(((IObserver)observer).ObservableTask, task));
        }
        public bool ObserverSubscritionExist()
        {
            return observers.Count > 0;
        }
        public static bool HasTaskBeenFinished(Task task)
        {
            return (task.IsCompleted || task.IsCanceled || task.IsFaulted);
        }
        public List<IObserver<Task>> Observers => observers;
        private bool HasObserverBeenRegistered(IObserver<Task> observer)
        {
            return observers.Contains(observer);
        }

        public List<Task> GetProcessingTasks()
        {
            List<Task> processingTasks = new List<Task>();
            var observersCopy = observers.ToList();
            observersCopy.ForEach((observer) =>
            {
                if (!(observer is null))
                {
                    var taskActionObserver = (IObserver)observer;

                    if (!(taskActionObserver.ObservableTask is null))
                        processingTasks.Add(taskActionObserver.ObservableTask);
                }
            });
            return processingTasks;
        }
        public async Task<bool> UnsubscribeObservers()
        {
            await CompleteQueueObservation();
            var observersCopy = Observers.ToList();

            foreach (IObserver<Task> observer in observersCopy)
            {
                if (!(observer is null))
                {
                    ((IObserver)observer).Unsubscribe();
                }
            }
            return true;
        }

        public async Task<List<bool>> CompleteQueueObservation()
        {
            var performedObservableTasks = new List<bool>();
            var observerCopy = Observers.ToList();

            foreach (IObserver<Task> observer in observerCopy)
            {
                ///Check null because observer could be unsubscribed in between by another process.
                if (!(observer is null))
                {
                    bool observed = await observeTransition((ITaskObserver)observer);
                    performedObservableTasks.Add(observed);
                }
            }
            return performedObservableTasks;
        }
        private async Task<bool> observeTransition(ITaskObserver observer)
        {
            bool observerCompleted = await observer.TaskStatusCompletedTransition;
            return observerCompleted;
        }
    }
}
