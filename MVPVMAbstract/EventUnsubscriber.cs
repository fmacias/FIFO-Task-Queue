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

namespace MVPVMAbstract
{
    public class EventUnsubscriber<T> : IEventUnsubscriber
    {
        private List<T> subscriptions;
        private T subscription;

        private EventUnsubscriber(List<T> subscriptions, T subscription)
        {
            this.subscriptions = subscriptions;
            this.subscription = subscription;
        }
        public static EventUnsubscriber<T> Create(List<T> subscriptions, T subscription)
        {
            return new EventUnsubscriber<T>(subscriptions, subscription);
        }
        public void Dispose()
        {
            if (subscriptions.Contains(subscription))
                subscriptions.Remove(subscription);
        }
    }
}
