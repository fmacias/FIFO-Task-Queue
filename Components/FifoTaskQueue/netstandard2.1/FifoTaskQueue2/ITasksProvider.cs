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
using System.Threading.Tasks;

namespace FifoTaskQueue2
{
    public interface ITasksProvider: IObservable<Task>
    {
        IEnumerable<Task> GetProcessingTasks();
        IEnumerable<IObserver<Task>> Subscriptions { get; }
        ISubscriberFactory SubscriberFactory { get; }
    }
}