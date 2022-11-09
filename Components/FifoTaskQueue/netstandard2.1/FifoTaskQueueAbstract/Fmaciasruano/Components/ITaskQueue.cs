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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface ITaskQueue
    {
        IActionObserver<TAction> Enqueue<TAction>(TAction action);
        IActionObserver<TAction> Enqueue<TAction,TArgs>(TAction action, params TArgs[] args);
        ITaskQueue Dequeue();
        Task<bool> Complete();
        ITaskQueue CancelAfter(int miliseconds);
        void CancelExecution();
        TaskScheduler TaskScheduler { get; }
        Task<IJobRunner> Start(ITaskObserver taskObserver);
        Task<IJobRunner> Continue(Task<IJobRunner> previousTask, ITaskObserver taskObserver);
        ITasksProvider Provider { get; }
        void UnsubscribeAll();
        bool CascadeCancelation { get; set; }
        int JobMaximalExceutionTime { get; set; }
    }
}
