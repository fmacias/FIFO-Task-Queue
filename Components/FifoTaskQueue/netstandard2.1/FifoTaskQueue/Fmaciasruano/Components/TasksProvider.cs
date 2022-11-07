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
        private readonly List<IObserver<Task>> observers;
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
            return Unsubscriber<Task>.Create(observers, observer);
        }

        public ITaskObserver[] Subscriptions => observers.Cast<ITaskObserver>().ToArray();

        public IObserver<Task>[] Observers => observers.ToArray();

        private bool HasObserverBeenRegistered(IObserver<Task> observer)
        {
            return observers.Contains(observer);
        }
    }
}
