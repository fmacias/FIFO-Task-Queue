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
using fmacias.Components.FifoTaskQueue;
using fmacias.Components.FifoTaskQueueAbstract;

namespace fmacias.Tests
{
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        private FifoTaskQueue CreateTaskQueue()
        {
            return FifoTaskQueue.Create(
                TaskShedulerWraper.Create().FromCurrentWorker(),
                LogManager.GetCurrentClassLogger());
        }
        [Test()]
        public void CreateTest()
        {
            Assert.IsInstanceOf<ITaskQueue>(CreateTaskQueue());
        }
        [Test()]
        public void CompleteAtSyncMethodTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue
                .Run(
                    queue.Define(() =>
                    {
                        Task.Delay(500).Wait();
                    }).OnCompleteCallback((object sender) =>
                    {
                        var observer = (ITaskObserver<Task>)sender;
                        Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                    })
                ).Run(queue.Define(() =>
                {
                    Task.Delay(500).Wait();
                }).OnCompleteCallback((object sender) =>
                {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                })
                ).Run(queue.Define(() =>
                {
                    Task.Delay(500).Wait();
                }).OnCompleteCallback((object sender) =>
                {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                }));
            }
        }

        [Test()]
        public async Task CompleteAtAsyncMethod()
        {
            bool firstRun = false;
            bool secondRun = false;
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue
                .Run(
                    queue.Define(() =>
                    {
                        firstRun = true;
                        Assert.IsTrue(firstRun == true && secondRun == false);
                    }).OnCompleteCallback((object sender) =>
                    {
                        var observer = (ITaskObserver<Task>)sender;
                        Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                    })
                ).Run(queue.Define(() =>
                {
                    secondRun = true;
                    Assert.IsTrue(firstRun == true && secondRun == true);
                }).OnCompleteCallback((object sender) =>
                {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                })
                ).Run(queue.Define(() =>
                {
                    Task.Delay(500).Wait();
                }).OnCompleteCallback((object sender) =>
                {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                }));
                bool done = await queue.Complete();
                Assert.IsTrue(queue.Tasks.Count == 0, "All Tasks where disposed!");
            }
        }
        [Test()]
        public async Task ActionsWithArguments()
        {
            bool firstRun = false;
            bool secondRun = false;
            bool thirdRun = false;
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(
                    queue.Define((testx) =>{
                        firstRun = true;
                        Assert.IsTrue(firstRun == true && secondRun == false && thirdRun == false);
                    }).OnCompleteCallback((object sender) =>{
                        var observer = (ITaskObserver<Task>)sender;
                        Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                    }),new object())
                .Run(queue.Define((testx) =>{
                    secondRun = true;
                    Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == false);
                }).OnCompleteCallback((object sender) =>{
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                }), new object())
                .Run(queue.Define((testx) =>{
                    thirdRun = true;
                    Assert.IsTrue(firstRun == true && secondRun == true && thirdRun == true);
                }).OnCompleteCallback((object sender) =>{
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(observer.Status, TaskObserverStatus.Completed);
                }), new object());
                bool done = await queue.Complete();
            }
        }
        [Test()]
        public async Task ActionsWithVariablesAreUnmutableTest()
        {
            int countIterations = 0;
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    Task.Delay(1000).Wait();
                    countIterations++;
                    Assert.AreEqual(1, countIterations);
                    Assert.AreEqual(0, inputParamteres[0]);
                }), countIterations);

                queue.Run(queue.Define((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    countIterations++;
                    Assert.AreEqual(2, countIterations);
                    Assert.AreEqual(0, inputParamteres[0]);
                }), countIterations);

                queue.Run(queue.Define((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    countIterations++;
                    Assert.AreEqual(3, countIterations);
                    Assert.AreEqual(0, inputParamteres[0]);
                }), countIterations);

                bool done = await queue.Complete();
                Assert.IsTrue(queue.Tasks.Count == 0, "All Tasks where disposed!");
            }
        }
        /// <summary>
        /// Tasks were not finshed wenn ClearUp was invoked, so that they wont be cleaned up.
        /// </summary>
        [Test()]
        public void TasksNotFinishedTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define(() =>{Task.Delay(5000).Wait();}).OnCompleteCallback((object sender)=>{
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                queue.Run(queue.Define(() => { Task.Delay(2000).Wait(); }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                queue.Run(queue.Define(() => { Task.Delay(2000).Wait(); }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                Assert.IsTrue(queue.Tasks[0].IsCompleted == false &&
                queue.Tasks[1].IsCompleted == false &&
                queue.Tasks[2].IsCompleted == false);
            }
        }
        /// <summary>
        /// Tasks were finished. So that they were cleaned up.
        /// </summary>
        [Test()]
        public async Task TasksFinishedTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define(() => { }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                queue.Run(queue.Define(() => { }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                queue.Run(queue.Define(() => { }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                bool done = await queue.Complete();
            }
        }
        [Test()]
        public async Task CancelFirstActionTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define(() => {
                    Task.Delay(5000, queue.CancellationToken).Wait();
                }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.CompletedWithErrors, observer.Status);
                }));
                queue.Run(queue.Define(() => {}).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Canceled, observer.Status);
                }));
                queue.Run(queue.Define(() => {}).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Canceled, observer.Status);
                }));
                bool done = await queue.CancelAfter(2000);
            }
        }
        [Test()]
        public async Task ActionsWithParamteresObjectsAreMutableTest()
        {
            object[] objectsToShare = new object[3];
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define((args) => {
                    ((object[])args)[0] = "a";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at first iteration");
                }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }), objectsToShare);
                queue.Run(queue.Define((args) => {
                    ((object[])args)[1] = "b";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at second iteration");
                }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }), objectsToShare);
                queue.Run(queue.Define((args) => {
                    ((object[])args)[2] = "c";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at third iteration");
                }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }), objectsToShare);
                bool done = await queue.Complete();
                Assert.AreEqual("a b c", String.Join(" ", objectsToShare));
            }
        }
        [Test()]
        public async Task CancelSecondTaskTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define(() => {}).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Completed, observer.Status);
                }));
                queue.Run(queue.Define(() => {
                    Task.Delay(5000,queue.CancellationToken).Wait();
                }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.CompletedWithErrors, observer.Status);
                }));
                queue.Run(queue.Define(() => { }).OnCompleteCallback((object sender) => {
                    var observer = (ITaskObserver<Task>)sender;
                    Assert.AreEqual(TaskObserverStatus.Canceled, observer.Status);
                }));
                bool done = await queue.CancelAfter(2000);
            }
        }
    }
}