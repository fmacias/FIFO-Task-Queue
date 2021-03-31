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

namespace fmacias
{
    public class ObserverUnsubscriber<Task> : IDisposable
    {
        private List<IObserver<Task>> _observers;
        private IObserver<Task> _observer;

        internal ObserverUnsubscriber(List<IObserver<Task>> observers, IObserver<Task> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }
        public static ObserverUnsubscriber<Task>  Create(List<IObserver<Task>> observers, IObserver<Task> observer)
        {
            return new ObserverUnsubscriber<Task>(observers, observer);
        }
        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
