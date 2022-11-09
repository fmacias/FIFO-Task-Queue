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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class TasksProvider : ITasksProvider
    {
        private readonly List<IObserver<Task<IJobRunner>>> observers;
        private readonly ILogger logger;

        private TasksProvider(ILogger logger)
        {
            observers = new List<IObserver<Task<IJobRunner>>>();
            this.logger = logger;
        }

        public static TasksProvider Create(ILogger logger)
        {
            return new TasksProvider(logger);
        }

        public IDisposable Subscribe(IObserver<Task<IJobRunner>> observer)
        {
            if (!HasObserverBeenRegistered(observer))
                observers.Add(observer);
            return Unsubscriber<Task<IJobRunner>>.Create(observers, observer);
        }

        public ITaskObserver[] Subscriptions => observers.Cast<ITaskObserver>().ToArray();

        public IObserver<Task<IJobRunner>>[] Observers => observers.ToArray();

        private bool HasObserverBeenRegistered(IObserver<Task<IJobRunner>> observer)
        {
            return observers.Contains(observer);
        }
        public async Task<List<bool>> CompleteQueueObservation()
        {
            var performedObservableTasks = new List<bool>();
            var observerCopy = Observers.ToList();

            foreach (IObserver<Task<IJobRunner>> observer in observerCopy)
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
            return await observer.TaskStatusFinishedTransition;
        }
    }
}
