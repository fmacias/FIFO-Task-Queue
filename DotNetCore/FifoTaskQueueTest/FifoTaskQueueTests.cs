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
using NUnit.Framework;
using fmacias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using NLog;
using Moq;

namespace fmacias.Tests
{
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        private FifoTaskQueue CreateTaskQueue()
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            return FifoTaskQueue.Create(
                TaskShedulerWraper.Create().FromCurrentWorker(), 
                TasksProvider.Create(new List<Task>(), logger),
                logger);
        }
        [Test()]
        public void CreateTest()
        {
            Assert.IsInstanceOf<ITaskQueue>(CreateTaskQueue());
        }
        [Test()]
        public async Task CompleteTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            queue.Run(() =>
            {
                Task.Delay(500).Wait();
            });
            queue.Run(() =>
            {
                Task.Delay(500).Wait();
            });
            queue.Run(() =>
            {
                Task.Delay(500).Wait();
            });
            Assert.IsTrue(await queue.Complete());
            queue.Dispose();
        }
        /// <summary>
        /// Run and dispose the queue.
        /// FifoTaskQueue is one <see cref="IDisposable"/> object.
        /// </summary>
        [Test()]
        public void RunTheQueueItselfAtSynchronMethodTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool firstRun = false;
            bool secondRun = false;
            bool thirdRun = false;
            queue.Run(() =>
            {
                Task.Delay(500).Wait();
                firstRun = true;
                Assert.IsTrue(firstRun == true && secondRun == false && thirdRun == false);
            });
            queue.Run(() =>
            {
                Task.Delay(500).Wait();
                secondRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == false);
            });
            queue.Run(() =>
            {
                Task.Delay(1000).Wait();
                thirdRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == true);
            });
            queue.Dispose();
        }

        [Test()]
        public async Task RunTheQueueItselfAtAsyncMethodTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool firstRun = false;
            bool secondRun = false;
            bool thirdRun = false;
            queue.Run(() =>
            {
                firstRun = true;
                Assert.IsTrue(firstRun == true && secondRun == false && thirdRun == false);
            });
            queue.Run(() =>
            {
                secondRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == false);
            });
            queue.Run(() =>
            {
                thirdRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == true);
            });
            bool done = await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "first task completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "second task completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "third task completed");
            queue.Dispose();
            Assert.IsTrue(queue.Tasks.Count == 0, "All Tasks where disposed!");
        }
        [Test()]
        public async Task Run_WithParameters_RunOrderIsSequentialTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool firstRun = false;
            bool secondRun = false;
            bool thirdRun = false;
            queue.Run((testx) =>
            {
                firstRun = true;
                Assert.IsTrue(firstRun == true && secondRun == false && thirdRun == false);
            }, new object());
            queue.Run((testx) =>
            {
                secondRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == false);
            }, new object());
            queue.Run((testx) =>
            {
                thirdRun = true;
                Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == true);
            }, new object());
            bool done = await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "first task completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "second task completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "third task completed");
            queue.Dispose();
            Assert.IsTrue(queue.Tasks.Count == 0, "All Tasks where disposed!");
        }
        [Test()]
        public async Task Run_WithParameters_ParametersAreUnmutableTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            int countIterations = 0;
            queue.Run((parameters) =>
            {
                Task.Delay(1000).Wait();
                countIterations++;
                Assert.AreEqual(1, countIterations);
                Assert.AreEqual(0, parameters);
            }, countIterations);
            queue.Run((parameters) =>
            {
                countIterations++;
                Assert.AreEqual(2, countIterations);
                Assert.AreEqual(0, parameters);
            }, countIterations);
            queue.Run((parameters) =>
            {
                countIterations++;
                Assert.AreEqual(3, countIterations);
                Assert.AreEqual(0, parameters);
            }, countIterations);
            bool done = await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "Task 1 is completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "Task 2 is completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "Task 3 is completed");
            queue.Dispose();
            Assert.IsTrue(queue.Tasks.Count == 0, "All Tasks where disposed!");
        }
        /// <summary>
        /// Tasks were not finshed wenn ClearUp was invoked, so that they wont be cleaned up.
        /// </summary>
        [Test()]
        public void ClearUp_Tasks_TasksNotFinishedTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            queue.Run(() =>
            {
                Task.Delay(5000).Wait();
            });
            queue.Run(() =>
            {
                Task.Delay(2000).Wait();
            });
            queue.Run(() =>
            {
                Task.Delay(2000).Wait();
            });
            queue.ClearUpTasks();
            Assert.IsTrue(queue.Tasks[0].IsCompleted == false &&
                queue.Tasks[1].IsCompleted == false &&
                queue.Tasks[2].IsCompleted == false);
            queue.Dispose();
            
        }
        /// <summary>
        /// Tasks were finished. So that they were cleaned up.
        /// </summary>
        [Test()]
        public async Task ClearUp_Tasks_TasksFinishedTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            queue.Run(() => { });
            queue.Run(() => { });
            queue.Run(() => { });
            bool done = await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsCompleted == true &&
            queue.Tasks[1].IsCompleted == true &&
            queue.Tasks[2].IsCompleted == true);
            queue.ClearUpTasks();
            Assert.IsTrue(queue.Tasks.Count() == 0);
            queue.Dispose();
        }
        [Test()]
        public void IsActionAsyncTest()
        {
            Action asyncAction = async () => { };
            Action syncAction = () => { };
            Assert.IsTrue(asyncAction.Method.IsDefined(typeof(AsyncStateMachineAttribute), false));
            Assert.IsFalse(syncAction.Method.IsDefined(typeof(AsyncStateMachineAttribute), false));
        }
        /// <summary>
        /// Queue is composed by three tasks.
        /// The first two will be run asynchronously and the third one not.
        /// Given that the first two ones are slow, the third one will be finished before
        /// the previous two async operations were finished.
        /// 
        /// Each run will be runned secuentially, but as previous ones are async and take longer, will be finished later.
        /// </summary>
        [Test()]
        public async Task Run_DownloadAsyncTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool downloaded1 = false;
            bool downloaded2 = false;
            queue.Run(async () =>
            {
                using (Task<bool> downloading = Download(2000))
                {
                    downloaded1 = await downloading;
                }
                Assert.IsTrue(downloaded1, "First async action performed.");
            });
            queue.Run(async () =>
            {
                using (Task<bool> downloading = Download(2000))
                {
                    downloaded2 = await downloading;
                }
                Assert.IsTrue(downloaded2, "Second async action performed.");
            });
            queue.Run(() =>
            {
                Assert.IsTrue(downloaded1 == false && downloaded2 == false);
            });
            bool done = await queue.Complete();
            //wait for download finalization at async methods.
            Task.Delay(4500).Wait();
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "all task completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "all task completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "all task completed");
            Assert.IsTrue(downloaded1, "First Async. Task performed.");
            Assert.IsTrue(downloaded2, "Second Async. Task performed.");
            queue.Dispose();
        }
        [Test()]
        public async Task run_CancelTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool firstTaskFinished = false;
            bool secondTaskfinished = false;
            bool thirdTaskStarted = false;

            queue.Run(() =>
            {
                Task.Delay(5000, queue.CancellationToken).Wait();
                firstTaskFinished = true;
            });
            queue.Run((dummyObject) =>
            {
                secondTaskfinished = true;
            }, new object());
            queue.Run((dummyObject) =>
            {
                thirdTaskStarted = false;
            }, new object());
            bool done = await queue.CancelAfter(2000);
            Assert.IsTrue(queue.Tasks[0].IsFaulted, "First Task faulted");
            Assert.IsFalse(firstTaskFinished, "First Task's Action not terminated");
            Assert.IsTrue(queue.Tasks[1].IsCanceled, "Second Task Canceled");
            Assert.IsFalse(secondTaskfinished, "Second not finished");
            Assert.IsTrue(queue.Tasks[2].IsCanceled, "third Task Canceled");
            Assert.IsFalse(thirdTaskStarted, "third task not finished");
            queue.Dispose();
        }
        [Test()]
        public async Task Run_WithParameters_ShareObject()
        {
            object[] objectRerenceToShare = new object[3];
            FifoTaskQueue queue = CreateTaskQueue();
            queue.Run((sharedObject) =>
            {
                ((object[])sharedObject)[0] = "a";
                Assert.IsTrue(object.ReferenceEquals(sharedObject, objectRerenceToShare),
                    "object is the same at first iteration");
            }, objectRerenceToShare);
            queue.Run((sharedObject) =>
            {
                ((object[])sharedObject)[1] = "b";
                Assert.IsTrue(object.ReferenceEquals(sharedObject, objectRerenceToShare),
                    "object is the same at second iteration");
            }, objectRerenceToShare);
            queue.Run((sharedObject) =>
            {
                ((object[])sharedObject)[2] = "c";
                Assert.IsTrue(object.ReferenceEquals(sharedObject, objectRerenceToShare),
                    "object is the same at third iteration");
            }, objectRerenceToShare);
            bool done = await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "first task completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "second task completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "third task completed");
            Assert.AreEqual("a b c",String.Join(" ", objectRerenceToShare));
            queue.Dispose();
        }
        /// <summary>
        /// During the execution of the second task, a cancelation to the queue
        /// if the work takes longer than 2 seconds, has been sent.
        /// 
        /// But, as the task does not collect the cancelation token, the second task will be finished.
        /// </summary>
        /// <returns></returns>
        [Test()]
        public async Task Complete_SecondTaskRunnedUntilTheEndTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool taskExecuted = false;
            queue.Run(() => { });
            queue.Run(() => {
                Task.Delay(5000).Wait();
                taskExecuted = true;
            });
            queue.Run(() => { });
            queue.Run(() => { });
            int elapsedTimeToCancelQueue = 2000;
            await queue.CancelAfter(elapsedTimeToCancelQueue);
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "First Task Completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted && taskExecuted == true, "second Task Completed and executed");
            Assert.IsTrue(queue.Tasks[2].IsCanceled && queue.Tasks[3].IsCanceled, "Last tasks canceled");
            queue.ClearUpTasks();
            queue.Dispose();
        }
        /// <summary>
        /// During the execution of the second task, a cancelation to the queue
        /// if the work takes longer than 2 seconds, has been sent.
        /// 
        /// As the task collects the cancelation token, the second task wont be finished,
        /// allthough it status will be set to <see cref="TaskStatus.IsCompleted"/>,
        /// the exection will be broken.
        /// </summary>
        [Test()]
        public async Task Complete_SecondTaskBrokenTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool taskExecuted = false;
            queue.Run(() => { });
            queue.Run(() => {
                Task.Delay(5000, queue.CancellationToken).Wait();
                taskExecuted = true;
            });
            queue.Run(() => { });
            queue.Run(() => { });
            int elapsedTimeToCancelQueue = 2000;
            await queue.CancelAfter(elapsedTimeToCancelQueue);
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "First Task Completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted && taskExecuted == false, "second Task completed but broken.");
            Assert.IsTrue(queue.Tasks[2].IsCanceled && queue.Tasks[2].IsCanceled, "Last tasks canceled");
            Assert.IsTrue(queue.Tasks[3].IsCanceled && queue.Tasks[3].IsCanceled, "Last tasks canceled");
            queue.ClearUpTasks();
            queue.Dispose();
        }
        /// <summary>
        /// It is posible to invoke the CompleteTasks method several times, for example after
        /// creation of the first run, so that the queue keep traking the first task as soon as it has 
        /// been created.
        /// 
        /// Imagine, that you have a IEnummerable of dependent actions to perform and you what to track each one
        /// dinamically after each iteration.
        /// </summary>
        /// <returns></returns>
        [Test()]
        public async Task CompleteTasks_Called_After_Each_TaskTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            queue.Run(() => {
                Task.Delay(5000, queue.CancellationToken).Wait();
            });
            await queue.CancelAfter(2000);
            queue.Run(() => { });
            await queue.Complete();
            queue.Run(() => { });
            await queue.Complete();
            queue.Run(() => { });
            await queue.Complete();
            Assert.IsTrue(queue.Tasks[0].IsFaulted, "First Task Faulted");
            Assert.IsTrue(queue.Tasks[1].IsCanceled, "second Task completed");
            Assert.IsTrue(queue.Tasks[2].IsCanceled && queue.Tasks[3].IsCanceled, "Last two completed");
            queue.Dispose();
        }
        [Test()]
        public async Task Run_DownloadSyncTest()
        {
            FifoTaskQueue queue = CreateTaskQueue();
            bool downloaded1 = false;
            bool downloaded2 = false;
            queue.Run(() =>
            {
                using (Task<bool> downloading = Download(1000))
                {
                    downloading.Wait();
                    downloaded1 = downloading.Result;
                }
                Assert.IsTrue(downloaded1, "First async action performed.");
            });
            queue.Run(() =>
            {
                using (Task<bool> downloading = Download(1000))
                {
                    downloading.Wait();
                    downloaded2 = downloading.Result;
                }
                Assert.IsTrue(downloaded2, "Second async action performed.");
            });
            queue.Run(() =>
            {
                Assert.IsTrue(downloaded1 == true && downloaded2 == true);
            });
            bool done = await queue.Complete();
            //wait for download finalization at async methods.
            Assert.IsTrue(queue.Tasks[0].IsCompleted, "all task completed");
            Assert.IsTrue(queue.Tasks[1].IsCompleted, "all task completed");
            Assert.IsTrue(queue.Tasks[2].IsCompleted, "all task completed");
            Assert.IsTrue(downloaded1, "First Async. Task performed.");
            Assert.IsTrue(downloaded2, "Second Async. Task performed.");
            queue.Dispose();
        }
        public async Task<bool> Download(int miliseconds)
        {
            await Task.Delay(miliseconds);
            return true; ;
        }
    }
}