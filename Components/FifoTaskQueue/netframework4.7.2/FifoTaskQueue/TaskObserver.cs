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
        private readonly Task task;
        private readonly ILogger logger;

        private IDisposable unsubscriber;
        private Task<bool> taskStatusCompletedTransition = Task.Run(() => { return false; });
        private event EventHandler<long> ObservedEvent;
        private event CompleteCallBackEventHandler CompleteCallBackEvent;
        private event ErrorCallBackEventHandler ErrorCallBackEvent;


        public delegate void CompleteCallBackEventHandler(object sender);
        public delegate void ErrorCallBackEventHandler(object sender);

        private CompleteCallBackEventHandler CompleteCallBackDelegate;
        private ErrorCallBackEventHandler ErrorCallBackDelegate;

        private Dictionary<Delegate, object> eventSubscriptions;
        private TaskObserverStatus status;
        private bool Completed = false;
        public Task ObservableTask => task;
        public TaskObserverStatus Status { get; }

        private void HandelnOnCompleteCallback(object sender)
        {
            CompleteCallBackDelegate(sender);
        }
        private void HandelnOnErrorCallback(object sender)
        {
            ErrorCallBackDelegate(sender);
        }
        public void OnCompleteCallback(CompleteCallBackEventHandler completeCallbackDelegate)
        {
            this.CompleteCallBackDelegate = completeCallbackDelegate;
        }
        public void OnErrorCallback(ErrorCallBackEventHandler errorCallbackDelegate)
        {
            this.ErrorCallBackDelegate = errorCallbackDelegate;
        }
        private TaskObserver(Task task,ILogger logger)
        {
            this.task = task;
            status = TaskObserverStatus.Created;
            ObservedEvent += HandleObserved;
            CompleteCallBackEvent += HandelnOnCompleteCallback;
            ErrorCallBackEvent += HandelnOnErrorCallback;
            CompleteCallBackDelegate = (object sender) => { };
            ErrorCallBackDelegate = (object sender) => { };
            this.logger = logger;
        }
        public static TaskObserver Create(Task task, ILogger logger)
        {
            return new TaskObserver(task,logger);
        }
        public Task<bool> TaskStatusCompletedTransition => taskStatusCompletedTransition;

        public IDisposable Unsubscriber => unsubscriber;

        public void OnCompleted()
        {
            Completed = true;
            status = TaskObserverStatus.Completed;
            OnCompleteCallback();
        }
        public virtual void Subscribe(TasksProvider provider)
        {
            unsubscriber = provider.Subscribe(this);
        }
        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
            ObservedEvent -= HandleObserved;
            CompleteCallBackEvent -= HandelnOnCompleteCallback;
            ErrorCallBackEvent -= ErrorCallBackEvent;

        }
        public void OnError(Exception error)
        {
            status = TaskObserverStatus.CompletedWithErrors;
            Console.Write(error.ToString());
            OnErrorCallback();
        }
        public void OnNext(Task value)
        {
            if (!Object.ReferenceEquals(task, value))
            {
                return;
            }
            logger.Debug(string.Format("Task id: {0} Will be observe. State: {1}", task.Id, task.Status));
            status = TaskObserverStatus.Observed;
            PollingTaskStatusTransition();
        }
        private void PollingTaskStatusTransition()
        {
            taskStatusCompletedTransition.Wait();
            taskStatusCompletedTransition.Dispose();
            taskStatusCompletedTransition = Task.Run(()=> 
            {
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                TaskStatus currentStatus = task.Status;
                logger.Debug(string.Format("Task id: {0} initial status {1}", task.Id, task.Status));

                while (!(task.IsCompleted || task.IsCanceled || task.IsFaulted) && (watch.ElapsedMilliseconds <= MAXIMAL_TASK_WATCHER_ELAPSED_TIME_MS))
                {
                    if (currentStatus != task.Status)
                    {
                        logger.Debug(string.Format("Task id: {0} Status transition to {1}", task.Id, task.Status));
                        currentStatus = task.Status;
                    }
                }
                long executionTime = watch.ElapsedMilliseconds;
                logger.Debug(string.Format("Task id: {0},  final status {1}, Duration: {2}", task.Id, task.Status, executionTime));
                watch.Stop();
                OnPollingTaskStatusTransitionFinishied(executionTime);
                return true;
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
            CompleteCallBackEventHandler raiseEvent = CompleteCallBackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }
        protected void OnErrorCallback()
        {
            ErrorCallBackEventHandler raiseEvent = ErrorCallBackEvent;

            if (raiseEvent != null)
            {
                raiseEvent(this);
            }
        }
        private void HandleObserved(object sender, long executionTime)
        {
            switch (this.ObservableTask.Status)
            {
                case (TaskStatus.RanToCompletion):
                    this.OnCompleted();
                    break;
                case (TaskStatus.Running):
                    this.status = TaskObserverStatus.ExecutionTimeExceded;
                    this.OnCompleted();
                    break;
                case (TaskStatus.Faulted):
                    this.status = TaskObserverStatus.CompletedWithErrors;
                    this.OnCompleted();
                    break;
                case (TaskStatus.Canceled):
                    this.status = TaskObserverStatus.Canceled;
                    this.OnCompleted();
                    break;
                default:
                    this.status = TaskObserverStatus.Unkonown;
                    this.OnCompleted();
                    break;
            }
        }
    }
}
