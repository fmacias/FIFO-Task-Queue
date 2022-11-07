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
using EventAggregatorAbstract.Fmaciasruano.Components;

namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface ITaskQueue
    {
        IActionObserver<TAction> Enqueue<TAction>(TAction action);
        IActionObserver<TAction> Enqueue<TAction,TArgs>(TAction action, params TArgs[] args);
        Task<IJobRunner> Complete(params ITaskObserver[] observers);
        Task<ITaskQueue> CancelAfter(int miliseconds);
        void CancelExecution();
        TaskScheduler TaskScheduler { get; }
        CancellationToken CancellationToken { get; }
        Task<IJobRunner> Run(ITaskObserver taskObserver);
        Task<IJobRunner> Continue(Task<IJobRunner> previousTask, ITaskObserver taskObserver);
        ITaskQueue OnQueueFinishedCallback(IProcessEvent.ProcessEventHandler handler);
        void OnQueueFinished();
        ITasksProvider Provider { get; }
    }
}
