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

namespace fmacias.Components.FifoTaskQueue.Tests
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
            Assert.IsInstanceOf<FifoTaskQueueAbstract.ITaskQueue>(CreateTaskQueue());
        }
        [Test]
        public void DefineTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
            {
                var observer1 = queue.Define<Action>(() => { });
                Assert.IsInstanceOf<IActionObserver<Action>>(observer1);
                Assert.IsInstanceOf<IObserver>(observer1);
                Assert.IsInstanceOf<ITaskObserver>(observer1);
            }
            using (ITaskQueue queue = CreateTaskQueue())
            {
                var observer2 = queue.Define<Action<object>>((args) => { });
                Assert.IsInstanceOf<IActionObserver<Action<object>>>(observer2);
                Assert.IsInstanceOf<IObserver>(observer2);
                Assert.IsInstanceOf<ITaskObserver>(observer2);
            }
        }
        [Test]
        public void RunTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action>(() => { }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => { }).OnCompleteCallback((object sender) => {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));
            }
        }
        [Test()]
        public void CompleteAtSyncMethodTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action>(() => { Task.Delay(500).Wait();}).OnCompleteCallback((object sender) =>{
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => { Task.Delay(500).Wait(); }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual((sender as IObserver).Status, ObserverStatus.Completed);
                }));

                queue.Run(queue.Define<Action>(() => { Task.Delay(500).Wait(); }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual((sender as IObserver).Status, ObserverStatus.Completed);
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
                queue.Run(queue.Define<Action>(() =>
                {
                    firstRun = true;
                    Assert.IsTrue(firstRun == true && secondRun == false);
                }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() =>
                {
                    secondRun = true;
                    Assert.IsTrue(firstRun == true && secondRun == true);
                }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));
                
                queue.Run(queue.Define<Action>(() => 
                {
                }).OnCompleteCallback((object sender) =>
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));
                bool done = await queue.Complete();
            }
        }
        /// <summary>
        /// Input variables passed as params are treated as variables.
        /// 
        /// </summary>
        /// <returns></returns>
        [Test()]
        public async Task ArgumentsForVariablesAreUnmutable()
        {
            int intNumber = 0;
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action<object>>((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    int intNumber = (int)inputParamteres[0];
                    intNumber++;
                    Assert.AreEqual(1, intNumber++);
                }), intNumber);

                queue.Run(queue.Define<Action<object>>((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    int intNumber = (int)inputParamteres[0];
                    intNumber++;
                    Assert.AreEqual(1, intNumber++);
                }), intNumber);

                queue.Run(queue.Define<Action<object>>((args) =>
                {
                    object[] inputParamteres = (object[])args;
                    int intNumber = (int)inputParamteres[0];
                    intNumber++;
                    Assert.AreEqual(1, intNumber++);
                }), intNumber);
                bool done = await queue.Complete();
            }
        }
        /// <summary>
        /// Tasks were not finshed yet
        /// </summary>
        [Test()]
        public void TasksNotFinishedTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action>(() => 
                { 
                    Task.Delay(5000).Wait(); 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                    Task.Delay(2000).Wait(); 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                    Task.Delay(2000).Wait(); 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                Assert.IsTrue(queue.Tasks[0].IsCompleted == false &&
                queue.Tasks[1].IsCompleted == false &&
                queue.Tasks[2].IsCompleted == false);
            }
        }
        /// <summary>
        /// Tasks are finished really fast and are finished before disposing.
        /// They will be cleard on diposing.
        /// </summary>
        [Test()]
        public async Task TasksFinishedTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));
                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));
                bool done = await queue.Complete();
                Assert.IsTrue(queue.Tasks.Count == 0);
            }
        }
        [Test()]
        public async Task CancelFirstActionTest()
        {
            using (FifoTaskQueue queue = CreateTaskQueue())
            {
                queue.Run(queue.Define<Action>(() => 
                {
                    Task.Delay(5000, queue.CancellationToken).Wait();
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.CompletedWithErrors, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Canceled, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Canceled, (sender as IObserver).Status);
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
                queue.Run(queue.Define<Action<object>>((args) => 
                {
                    ((object[])args)[0] = "a";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at first iteration");
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }), objectsToShare);

                queue.Run(queue.Define<Action<object>>((args) => 
                {
                    ((object[])args)[1] = "b";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at second iteration");
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }), objectsToShare);

                queue.Run(queue.Define<Action<object>>((args) => 
                {
                    ((object[])args)[2] = "c";
                    Assert.IsTrue(object.ReferenceEquals(args, objectsToShare),
                        "object is the same at third iteration");
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
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
                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                {
                    Task.Delay(5000, queue.CancellationToken).Wait();
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.CompletedWithErrors, (sender as IObserver).Status);
                }));

                queue.Run(queue.Define<Action>(() => 
                { 
                }).OnCompleteCallback((object sender) => 
                {
                    Assert.AreEqual(ObserverStatus.Canceled, (sender as IObserver).Status);
                }));

                bool done = await queue.CancelAfter(2000);
            }
        }
    }
}