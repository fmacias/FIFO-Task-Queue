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
    internal class TaskObserver<TAction> : IActionObserver<TAction>
    {
        private const int MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS = 600000;
        private Task runningTask;
        private readonly ILogger logger;
        private TAction action;
        private IDisposable unsubscriber;
        private Task<bool> taskStatusCompletedTransition = Task.Run(() => { return false; });
        private event EventHandler<long> CompletedEvent;
        private event EventHandler<Exception> ErrorEvent;
        private event IActionObserver<TAction>.CompleteCallBackEventHandler CompletedCallbackEvent;
        private event IActionObserver<TAction>.ErrorCallBackEventHandler ErrorCallBackEvent;
        private IActionObserver<TAction>.CompleteCallBackEventHandler CompletedCallBackDelegate;
        private IActionObserver<TAction>.ErrorCallBackEventHandler ErrorCallBackDelegate;
        public Task ObservableTask => runningTask;
        public ObserverStatus Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        private TaskObserver(ILogger logger)
        {
            Status = ObserverStatus.Created;
            CompletedEvent += HandleCompleted;
            ErrorEvent += HandleError;
            CompletedCallbackEvent += HandelnCompletedCallback;
            ErrorCallBackEvent += HandelnErrorCallback;
            CompletedCallBackDelegate = (object sender) => { };
            ErrorCallBackDelegate = (object sender) => { };
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static TaskObserver<TAction> Create(ILogger logger)
        {
            return new TaskObserver<TAction>(logger);
        }


        /// <summary>
        /// Cleapup resourced managed by the Observer and trigger a callback to the 
        /// corresponging Event-Subscriber.
        /// 
        /// Notes:
        /// . Task cleaned up before callback.
        /// . Obeserver unsubscription after callback completion.
        /// . This method is triggered after Task Completion. 
        /// As the Observed Task is being managed by the Provider and consumed by the Queue, 
        /// which ensures that the subscriptions are being properly cleaned up on 
        /// Disposing to avoid memory leak problems, and given that the Queue(Consumer) is 
        /// able trigger a cascade cancellation, is necessary to catch, ignoring the 
        /// Exceptions(TaskCancelledException inside of a AggregationException ), 
        /// on Waiting for Task finalization(from the point view of the Observer is 
        /// already finalized).
        /// Interface <see cref="ITaskObserver"/> implements <see cref="IObserver<Task>"/>
        /// </summary>
        public void OnCompleted()
        {
            logger.Debug(String.Format("Task {0} observation completed ", ObservableTask.Id));
            try
            {
                this.ObservableTask.Wait();
            }
            catch{ }
            this.ObservableTask.Dispose();
            OnCompleteCallback();
            Unsubscribe();
        }

        /// <summary>
        /// Interface <see cref="ITaskObserver"/> implements <see cref="IObserver<Task>"/>
        /// </summary>
        public void OnError(Exception error)
        {
            this.ObservableTask.Dispose();
            Status = ObserverStatus.CompletedWithErrors;
            Console.Write(error.ToString());
            OnErrorCallback(error);
            Unsubscribe();
        }

        /// <summary>
        /// Interface <see cref="ITaskObserver"/> implements <see cref="IObserver<Task>"/>
        /// </summary>
        public void OnNext(Task value)
        {
            runningTask = value;
            logger.Debug(string.Format("Task id: {0} Will be observe. State: {1}", runningTask.Id, runningTask.Status));
            Status = ObserverStatus.Observed;
            PollingTaskStatusTransition();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public virtual void Subscribe(ITasksProvider provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber?.Dispose();
            CompletedEvent -= HandleCompleted;
            ErrorEvent -= HandleError;
            CompletedCallbackEvent -= HandelnCompletedCallback;
            ErrorCallBackEvent -= HandelnErrorCallback;

            if (ObservableTask is null)
                logger.Debug("Observer of non started Task unsubscribed!");
            else
                logger.Debug(String.Format("Observer of Task {0} unsubscribed!", ObservableTask.Id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IActionObserver<TAction> SetAction(TAction action)
        {
            this.action = action;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TAction GetAction()
        {
            return action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorCallbackDelegate"></param>
        /// <returns></returns>
        public IActionObserver<TAction> OnErrorCallback(IActionObserver<TAction>.ErrorCallBackEventHandler errorCallbackDelegate)
        {
            this.ErrorCallBackDelegate = errorCallbackDelegate;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="completeCallbackDelegate"></param>
        /// <returns></returns>
        public IActionObserver<TAction> OnCompleteCallback(IActionObserver<TAction>.CompleteCallBackEventHandler completeCallbackDelegate)
        {
            this.CompletedCallBackDelegate = completeCallbackDelegate;
            return this;
        }

        public Task<bool> TaskStatusCompletedTransition => taskStatusCompletedTransition;

        public IDisposable Unsubscriber => unsubscriber;
        
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

                    while (!(IsTaskBeingProcessed()) && (!MaximalElapsedTimeExceded(watch)))
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
                    OnPollingTaskStatusTransitionError(e);
                }
                return processed;
            });

            bool IsTaskBeingProcessed()
            {
                return runningTask.IsCompleted || runningTask.IsCanceled || runningTask.IsFaulted;
            }

            static bool MaximalElapsedTimeExceded(System.Diagnostics.Stopwatch watch)
            {
                return watch.ElapsedMilliseconds > MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS;
            }
        }

        protected virtual void OnPollingTaskStatusTransitionFinishied(long executionTime)
        {
            EventHandler<long> raiseEvent = CompletedEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this, executionTime);
            }
        }
        protected virtual void OnPollingTaskStatusTransitionError(Exception e)
        {
            EventHandler<Exception> raiseEvent = ErrorEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this, e);
            }
        }
        private void HandleCompleted(object sender, long executionTime)
        {
            var oSender = (IObserver)sender;
            switch (oSender.ObservableTask.Status)
            {
                case TaskStatus.RanToCompletion:
                    oSender.Status = ObserverStatus.Completed;
                    break;
                case TaskStatus.Running:
                    oSender.Status = ObserverStatus.ExecutionTimeExceded;
                    break;
                case TaskStatus.Faulted:
                    oSender.Status = ObserverStatus.CompletedWithErrors;
                    break;
                case (TaskStatus.Canceled):
                    oSender.Status = ObserverStatus.Canceled;
                    break;
                default:
                    this.Status = ObserverStatus.Unkonown;
                    break;
            }
            oSender.OnCompleted();
        }
        private void HandleError(object sender, Exception error)
        {
            var oSender = (IObserver)sender;
            oSender.OnError(error);
        }
        protected void OnCompleteCallback()
        {
            IActionObserver<TAction>.CompleteCallBackEventHandler raiseEvent = CompletedCallbackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }

        protected void OnErrorCallback(Exception error)
        {
            IActionObserver<TAction>.ErrorCallBackEventHandler raiseEvent = ErrorCallBackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }
        private void HandelnCompletedCallback(object sender)
        {
            CompletedCallBackDelegate(sender);
        }

        private void HandelnErrorCallback(object sender)
        {
            ErrorCallBackDelegate(sender);
        }
    }
}
