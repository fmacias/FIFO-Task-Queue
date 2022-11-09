using System;
using System.Threading;
using System.Threading.Tasks;
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;
using NUnit.Framework;

namespace FifoTaskQueueNC5_0.Test.Fmaciasruano.Components
{
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        private ITaskQueue fifoTaskQueue;

        [SetUp]
        public void ResetEventsOutput()
        {
            fifoTaskQueue = FifoTaskQueue.Fmaciasruano.Components.FifoTaskQueue.Create(
                TaskScheduler.Current,
                TasksProvider.Create(LogManager.GetCurrentClassLogger()),
                LogManager.GetCurrentClassLogger()
            );
        }

        [Test()]
        public void CreateTest()
        {
            Assert.IsInstanceOf<ITaskQueue>(FifoTaskQueue.Fmaciasruano.Components.FifoTaskQueue.Create(
                    TaskScheduler.Current,
                    TasksProvider.Create(LogManager.GetCurrentClassLogger()),
                    LogManager.GetCurrentClassLogger()
                ));
        }

        [Test()]
        public void EnqueueActionNoArgumentsTest()
        {
            bool done1 = false;
            bool done2 = false;
            IActionObserver<Action> observer1 = fifoTaskQueue.Enqueue<Action>(() =>
            {
                done1 = true;
                
            });

            IActionObserver<Action> observer2 = fifoTaskQueue.Enqueue<Action>(() =>
            {
                done2 = true;
            
            });
            Assert.IsFalse(object.ReferenceEquals(observer1.Runner, observer2.Runner));
            observer1.Runner.Run();
            observer2.Runner.Run();
            observer1.Unsubscribe();
            observer2.Unsubscribe();
            Assert.IsTrue(done1);
            Assert.IsTrue(done2);
        }

        [Test()]
        public void EnqueueActionArgumentsTest()
        {
            IActionObserver<Action<int>> observer1 = fifoTaskQueue.Enqueue<Action<int>,int>((args) =>
            {
                Assert.AreEqual(1,args);
            },1);

            Assert.IsInstanceOf<IJobRunner>(observer1.Runner);

            int[] arrayParams = {1, 2};

            IActionObserver<Action<int[]>> observer2 = fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
                Assert.AreEqual(1, args[0]);
                Assert.AreEqual(2, args[1]);
            }, arrayParams);
            Assert.IsFalse(object.ReferenceEquals(observer1.Runner, observer2.Runner));
            Assert.IsInstanceOf<IJobRunner>(observer1.Runner);
            Assert.IsInstanceOf<IJobRunner>(observer2.Runner);
            observer1.Runner.Run();
            observer1.Unsubscribe();
            observer2.Runner.Run();
            observer2.Unsubscribe();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
        }
        [Test()]
        public async Task Complete_SyncActionAndCallbacksAreSequentiallyInvoked()
        {
            bool firstActionDone = false;
            bool firstCallback = false;
            bool secondActionDone = false;
            bool secondCallback = false;

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
                firstActionDone = true;
                Assert.AreEqual(false, secondActionDone, "Second Action not performed");
                Assert.AreEqual(false, firstCallback, "First Callback not performed");
             }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
             {
                firstCallback = true;
                Assert.AreEqual(true, firstActionDone, "First Action performed");
             }).Name="Queue 1";

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(1000).Wait();
                secondActionDone = true;
                Assert.AreEqual(true, firstActionDone, "Second Action performed first");

            }).OnCompleteCallback((object sender) => {
                Task.Delay(1000).Wait();
                secondCallback = true;
                Assert.AreEqual(true, firstActionDone, "First Action performed");
                Assert.AreEqual(true, secondActionDone, "First Action performed");
            }).Name="Queue 2";

            //Dequeue can be runned after enqueue or when ever, each time runs the next queue.
            //Does not block the thread, so that it can be run after  enqueue.
            fifoTaskQueue.Dequeue();
            fifoTaskQueue.Dequeue();
            
            // Await task finalization for the currently loaded Task in queue
            bool alloberversHaveBeenFinalized = await fifoTaskQueue.Complete();

            //Ensure that the Observers are being unsubscribed after callbacks
            Task.Delay(1100).Wait();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);

            //
            Assert.AreEqual(true, firstActionDone);
            Assert.AreEqual(true,firstCallback);
            Assert.AreEqual(true, secondActionDone);
            Assert.AreEqual(true, secondCallback);
        }
        [Test]
        public void Complete_TaskThrowsException()
		{
            bool firstTaskCompleted = true;
            bool secondTaskCompleted = false;
            bool thirdTaskCanceled = false;

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
            }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
            {
                Assert.IsInstanceOf<TaskObserver<Action<int[]>>>(sender);
                TaskObserver<Action<int[]>> currentObserver = (TaskObserver<Action<int[]>>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                Assert.AreEqual(TaskStatus.RanToCompletion, currentObserver.RunningTask.Status);
                firstTaskCompleted = true;
            }).Name = "Queue 1";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(1000).Wait();

            }).OnCompleteCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                secondTaskCompleted = true;
            }).OnErrorCallback((object sender) => {
            }).Name = "Queue 2";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                throw new Exception("Error happened");
            }).OnCompleteCallback((object sender) => {
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.CompletedWithErrors, currentObserver.Status);
                thirdTaskCanceled = true;
            }).Name = "Queue 3";

            fifoTaskQueue.Dequeue();

            //Ensure that the Observers are being unsubscribed after callbacks
            Task.Delay(3000).Wait();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.AreEqual(true, firstTaskCompleted);
            Assert.AreEqual(true, secondTaskCompleted);
            Assert.AreEqual(true, thirdTaskCanceled);
        }
        /// <summary>
        /// This situation should be avoided. Run an async Task into the queue does not make sense,
        /// but could be a real situation.
        /// 
        /// To solve this situation, it is posible to add a new job in between with a timeout.
        /// 
        /// </summary>
        /// <returns></returns>
        [Test()]
        public async Task Comlete_AsyncTaskRunnedInBetween()
        {
            bool firstActionDone = false;
            bool secondActionDone = false;
            bool firstAsyncOPerationDone = false;
            bool timeOutInBetween = false;

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    firstAsyncOPerationDone = true;
                });
                firstActionDone = true;
                Assert.AreEqual(false, firstAsyncOPerationDone, "Second Action performed first");
                Assert.AreEqual(false, secondActionDone, "Second Action performed first");
            }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(true, firstActionDone, "First Action performed");
            }).Name="Queue 1";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(3500).Wait();
                timeOutInBetween = true;
                Assert.AreEqual(true, firstAsyncOPerationDone, "Async operation of first task performed");
            }).Name = "Queue 2: Try to Finalize previous async task";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(100).Wait();
                secondActionDone = true;
                Assert.AreEqual(true, timeOutInBetween, "Timeout in between done");
                Assert.AreEqual(true, firstActionDone, "Second Action performed first");
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(true, secondActionDone, "Second Action performed");
            }).Name="Queue 3";

            fifoTaskQueue.Dequeue();

            await fifoTaskQueue.Complete();

            //Ensure that the Observers are being unsubscribed after callbacks
            Task.Delay(1000).Wait();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Test()]
        public void CancelAfter_CascadeCancelation()
        {
            bool firstTaskCompleted = true;
            bool secondTaskCompleted = false;
            bool secondTaskRunned = false;
            bool thirdTaskCanceled = false;
            bool fourthTaskCanceled = false;


            fifoTaskQueue.CancelAfter(1000);

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
            }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
            {
                Assert.IsInstanceOf<TaskObserver<Action<int[]>>>(sender);
                TaskObserver<Action<int[]>> currentObserver = (TaskObserver<Action<int[]>>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                Assert.AreEqual(TaskStatus.RanToCompletion, currentObserver.RunningTask.Status);
                firstTaskCompleted = true;
            }).Name = "Queue 1";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(1500).Wait();
                secondTaskRunned = true;

            }).OnCompleteCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                secondTaskCompleted = true;
            }).OnErrorCallback((object sender) => {
            }).Name = "Queue 2";
            
            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();
            }).OnCompleteCallback((object sender) => {
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Canceled, currentObserver.Status);
                thirdTaskCanceled = true;
            }).Name = "Queue 3";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();

            }).OnCompleteCallback((object sender) => {
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Canceled, currentObserver.Status);
                fourthTaskCanceled = true;
            }).Name = "Queue 4";

            fifoTaskQueue.Dequeue();

            //Ensure that the Observers are being unsubscribed after callbacks
            Thread.Sleep(5000);
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.AreEqual(true, firstTaskCompleted);
            Assert.AreEqual(true, secondTaskRunned);
            Assert.AreEqual(true,secondTaskCompleted,  "But, finalized as Completed.");
            Assert.AreEqual(true, thirdTaskCanceled, "Third task canceled");
            Assert.AreEqual(true, fourthTaskCanceled, "fourth task canceled no executed");
        }

        [Test()]
        public void CancelAfter_LastCancelation()
        {
            bool firstTaskCompleted = true;
            bool secondTaskCompleted = false;
            bool secondTaskRunned = false;
            bool thirdTaskCanceled = false;
            bool fourthTaskCompleted = false;

            fifoTaskQueue.CascadeCancelation = false;

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
            }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
            {
                Assert.IsInstanceOf<TaskObserver<Action<int[]>>>(sender);
                TaskObserver<Action<int[]>> currentObserver = (TaskObserver<Action<int[]>>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                Assert.AreEqual(TaskStatus.RanToCompletion, currentObserver.RunningTask.Status);
                firstTaskCompleted = true;
            }).Name = "Queue 1";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(1500).Wait();
                secondTaskRunned = true;

            }).OnCompleteCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                secondTaskCompleted = true;
            }).OnErrorCallback((object sender) => {
            }).Name = "Queue 2";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();
            }).OnCompleteCallback((object sender) => {
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Canceled, currentObserver.Status);
                thirdTaskCanceled = true;
            }).Name = "Queue 3";

            fifoTaskQueue.Dequeue();
            ///notice, hast to be run after dequeue. Cancel last enqueued task.
            fifoTaskQueue.CancelAfter(1000);

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();

            }).OnCompleteCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                fourthTaskCompleted = true;
            }).OnErrorCallback((object sender) => {
            }).Name = "Queue 4";

            fifoTaskQueue.Dequeue();

            //Ensure that the Observers are being unsubscribed after callbacks
            Task.Delay(5000).Wait();
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.AreEqual(true, firstTaskCompleted);
            Assert.AreEqual(true, secondTaskRunned);
            Assert.AreEqual(true, secondTaskCompleted);
            Assert.AreEqual(true, thirdTaskCanceled, "Third task canceled");
            Assert.AreEqual(true, fourthTaskCompleted, "fourth task canceled no executed");
        }
        [Test()]
        public void CancelOneJobAfterTimeout()
        {
            bool firstTaskCompleted = true;
            bool secondTaskCompleted = false;
            bool secondTaskRunned = false;
            bool thirdTaskCanceled = false;
            bool fourthTaskCanceled = false;

            fifoTaskQueue.CascadeCancelation = false;
            fifoTaskQueue.JobMaximalExceutionTime = 1800;

            fifoTaskQueue.Enqueue<Action<int[]>, int[]>((args) =>
            {
            }, new int[] { 1, 2 }).OnCompleteCallback((object sender) =>
            {
                Assert.IsInstanceOf<TaskObserver<Action<int[]>>>(sender);
                TaskObserver<Action<int[]>> currentObserver = (TaskObserver<Action<int[]>>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                Assert.AreEqual(TaskStatus.RanToCompletion, currentObserver.RunningTask.Status);
                firstTaskCompleted = true;
            }).Name = "Queue 1";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(1500).Wait();
                secondTaskRunned = true;

            }).OnCompleteCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.Completed, currentObserver.Status);
                secondTaskCompleted = true;
            }).OnErrorCallback((object sender) => {
            }).Name = "Queue 2";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();
            }).OnCompleteCallback((object sender) => {
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.ExecutionTimeExceeded, currentObserver.Status);
                thirdTaskCanceled = true;
            }).Name = "Queue 3";

            fifoTaskQueue.Dequeue();

            fifoTaskQueue.Enqueue<Action>(() =>
            {
                Task.Delay(2000).Wait();

            }).OnCompleteCallback((object sender) => {
                var a = 1;
            }).OnErrorCallback((object sender) => {
                TaskObserver<Action> currentObserver = (TaskObserver<Action>)sender;
                Assert.AreEqual(ObserverStatus.ExecutionTimeExceeded, currentObserver.Status);
                fourthTaskCanceled = true;
            }).Name = "Queue 4";

            fifoTaskQueue.Dequeue();

            //Ensure that the Observers are being unsubscribed after callbacks
            Thread.Sleep(5000);
            Assert.AreEqual(0, fifoTaskQueue.Provider.Subscriptions.Length);
            Assert.AreEqual(true, firstTaskCompleted);
            Assert.AreEqual(true, secondTaskRunned);
            Assert.AreEqual(true, secondTaskCompleted);
            Assert.AreEqual(true, thirdTaskCanceled, "Third task canceled");
            Assert.AreEqual(true, fourthTaskCanceled, "fourth task canceled no executed");
        }
    }
}