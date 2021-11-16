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
using System.Reflection;
using System.Threading.Tasks;

namespace fmacias.Components.FifoTaskQueue
{
    internal class TaskObserver : ITaskObserver<Task>
    {
        private const int MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS = 10000;
        private Task runningTask;
        private readonly ILogger logger;
        private Action action;
        private Action<object> actionParams;

        private IDisposable unsubscriber;
        private Task<bool> taskStatusCompletedTransition = Task.Run(() => { return false; });
        private event EventHandler<long> ObservedEvent;
        private event ITaskObserver<Task>.CompleteCallBackEventHandler CompleteCallBackEvent;
        private event ITaskObserver<Task>.ErrorCallBackEventHandler ErrorCallBackEvent;
        private ITaskObserver<Task>.CompleteCallBackEventHandler CompleteCallBackDelegate;
        private ITaskObserver<Task>.ErrorCallBackEventHandler ErrorCallBackDelegate;
        private TaskObserverStatus status;
        public Task ObservableTask => runningTask;
        public TaskObserverStatus Status { get; set; }

        private void HandelnOnCompleteCallback(object sender)
        {
            CompleteCallBackDelegate(sender);
        }
        private void HandelnOnErrorCallback(object sender)
        {
            ErrorCallBackDelegate(sender);
        }
        public ITaskObserver<Task> OnCompleteCallback(ITaskObserver<Task>.CompleteCallBackEventHandler completeCallbackDelegate)
        {
            this.CompleteCallBackDelegate = completeCallbackDelegate;
            return this;
        }
        public ITaskObserver<Task> OnErrorCallback(ITaskObserver<Task>.ErrorCallBackEventHandler errorCallbackDelegate)
        {
            this.ErrorCallBackDelegate = errorCallbackDelegate;
            return this;
        }
        private TaskObserver(ILogger logger)
        {
            status = TaskObserverStatus.Created;
            ObservedEvent += HandleObserved;
            CompleteCallBackEvent += HandelnOnCompleteCallback;
            ErrorCallBackEvent += HandelnOnErrorCallback;
            CompleteCallBackDelegate = (object sender) => { };
            ErrorCallBackDelegate = (object sender) => { };
            this.logger = logger;
        }
        public static TaskObserver Create(ILogger logger)
        {
            return new TaskObserver(logger);
        }
        public Task<bool> TaskStatusCompletedTransition => taskStatusCompletedTransition;
        public IDisposable Unsubscriber => unsubscriber;
        public Action Action { get => action; set => action = value; }
        public Action<object> ActionParams { get => actionParams; set => actionParams = value; }
        public void OnCompleted()
        {
            OnCompleteCallback();
            Unsubscribe();
        }
        public virtual void Subscribe(ITasksProvider provider)
        {
            unsubscriber = provider.Subscribe(this);
        }
        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
            ObservedEvent -= HandleObserved;
            CompleteCallBackEvent -= HandelnOnCompleteCallback;
            ErrorCallBackEvent -= HandelnOnErrorCallback;
        }
        public void OnError(Exception error)
        {
            status = TaskObserverStatus.CompletedWithErrors;
            Console.Write(error.ToString());
            OnErrorCallback();
        }
        public void OnNext(Task value)
        {
            runningTask = value;
            logger.Debug(string.Format("Task id: {0} Will be observe. State: {1}", runningTask.Id, runningTask.Status));
            status = TaskObserverStatus.Observed;
            PollingTaskStatusTransition();
        }
        private void PollingTaskStatusTransition()
        {
            taskStatusCompletedTransition.Wait();
            taskStatusCompletedTransition.Dispose();
            taskStatusCompletedTransition = Task.Run(() =>
            {
                bool processed = false;
                try
                {
                    System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                    TaskStatus currentStatus = runningTask.Status;
                    logger.Debug(string.Format("Task id: {0} initial status {1}", runningTask.Id, runningTask.Status));
                    while (!(runningTask.IsCompleted || runningTask.IsCanceled || runningTask.IsFaulted) && (watch.ElapsedMilliseconds <= MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS))
                    {
                        if (currentStatus != runningTask.Status)
                        {
                            logger.Debug(string.Format("Task id: {0} Status transition to {1}", runningTask.Id, runningTask.Status));
                            currentStatus = runningTask.Status;
                        }
                    }
                    long executionTime = watch.ElapsedMilliseconds;
                    logger.Debug(string.Format("Task id: {0},  final status {1}, Duration: {2}", runningTask.Id, runningTask.Status, executionTime));
                    watch.Stop();
                    processed = true;
                    OnPollingTaskStatusTransitionFinishied(executionTime);
                }
                catch (Exception e)
                {
                    OnErrorCallback();
                }
                return processed;
            });
        }
        protected virtual void OnPollingTaskStatusTransitionFinishied(long executionTime)
        {
            EventHandler<long> raiseEvent = ObservedEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this, executionTime);
            }
        }
        protected void OnCompleteCallback()
        {
            ITaskObserver<Task>.CompleteCallBackEventHandler raiseEvent = CompleteCallBackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }
        protected void OnErrorCallback()
        {
            ITaskObserver<Task>.ErrorCallBackEventHandler raiseEvent = ErrorCallBackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }
        private void HandleObserved(object sender, long executionTime)
        {
            ITaskObserver<Task> oSender = (ITaskObserver<Task>)sender;
            switch (oSender.ObservableTask.Status)
            {
                case TaskStatus.RanToCompletion:
                    oSender.Status = TaskObserverStatus.Completed;
                    break;
                case TaskStatus.Running:
                    oSender.Status = TaskObserverStatus.ExecutionTimeExceded;
                    break;
                case TaskStatus.Faulted:
                    oSender.Status = TaskObserverStatus.CompletedWithErrors;
                    break;
                case (TaskStatus.Canceled):
                    oSender.Status = TaskObserverStatus.Canceled;
                    break;
                default:
                    this.Status = TaskObserverStatus.Unkonown;
                    break;
            }
            oSender.OnCompleted();
        }
    }
}
