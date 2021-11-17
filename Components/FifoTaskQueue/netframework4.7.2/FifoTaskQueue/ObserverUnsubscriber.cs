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
using System;
using System.Collections.Generic;

namespace fmacias.Components.FifoTaskQueue
{
    internal class ObserverUnsubscriber<Task> : IDisposable
    {
        private List<ITaskObserver<Task>> observers;
        private ITaskObserver<Task> observer;

        private ObserverUnsubscriber(List<ITaskObserver<Task>> observers, ITaskObserver<Task> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }
        internal static ObserverUnsubscriber<Task> Create(List<ITaskObserver<Task>> observers, ITaskObserver<Task> observer)
        {
            return new ObserverUnsubscriber<Task>(observers, observer);
        }
        public void Dispose()
        {
            if (observers.Contains(observer))
                observers.Remove(observer);
        }
    }
}
