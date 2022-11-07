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

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class Unsubscriber<TAsk> : IDisposable
    {
        private readonly List<IObserver<TAsk>> observers;
        private readonly IObserver<TAsk> observer;

        private Unsubscriber(List<IObserver<TAsk>> observers, IObserver<TAsk> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }
        internal static Unsubscriber<TAsk> Create(List<IObserver<TAsk>> observers, IObserver<TAsk> observer)
        {
            return new Unsubscriber<TAsk>(observers, observer);
        }
        public void Dispose()
        {
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }
    }
}
