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
using FifoTaskQueue;
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using NLog;
using Moq;
using fmacias.Components.FifoTaskQueue;
using fmacias.Components.FifoTaskQueueAbstract;

namespace fmacias.Components.FifoTaskQueue.Tests
{
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        public class SharedObject{
            public string propertyOne { get; set; }
        }

        private FifoTaskQueue CreateTaskQueue()
        {
            return FifoTaskQueue.Create(
                Sheduler.Create().FromCurrentWorker(),
                LogManager.GetCurrentClassLogger());
        }

        [Test()]
        public void CreateTest()
        {
            Assert.IsInstanceOf<ITaskQueue>(CreateTaskQueue());
        }

        [Test]
        public void DefineTest()
        {
            ITaskQueue queue = CreateTaskQueue();

            var observer1 = queue.Define<Action>(() => { });
            Assert.IsInstanceOf<IActionObserver<Action>>(observer1);
            Assert.IsInstanceOf<IObserver>(observer1);
            Assert.IsInstanceOf<ITaskObserver>(observer1);


            var observer2 = queue.Define<Action<object>>((args) => { });
            Assert.IsInstanceOf<IActionObserver<Action<object>>>(observer2);
            Assert.IsInstanceOf<IObserver>(observer2);
            Assert.IsInstanceOf<ITaskObserver>(observer2);

            var observer3 = queue.Define<Action<int>>((args) => { });
            Assert.IsInstanceOf<IActionObserver<Action<int>>>(observer3);
            Assert.IsInstanceOf<IObserver>(observer3);
            Assert.IsInstanceOf<ITaskObserver>(observer3);
        }

        [Test]
        public void RunTest()
        {
            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action>(() => 
            { 
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }));

            queue.Run(queue.Define<Action>(() => 
            { 
            }).OnCompleteCallback((object sender) => {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }));
        }

        [Test()]
        public void CompleteAtSyncMethodTest()
        {
            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action>(() => 
            { 
                Task.Delay(500).Wait();
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }));

            queue.Run(queue.Define<Action>(() => 
            { 
                Task.Delay(500).Wait(); 
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual((sender as IObserver).Status, ObserverStatus.Completed);
            }));

            queue.Run(queue.Define<Action>(() => 
            { 
                Task.Delay(500).Wait(); 
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual((sender as IObserver).Status, ObserverStatus.Completed);
            }));
        }

        [Test()]
        public async Task VariablesNotPassedAsReferenceToTheActions()
        {
            bool firstRun = false;
            bool secondRun = false;

            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action<bool[]>>((args) =>
            {
                Assert.IsTrue(args[0] == false && args[1] == false);
                args[0] = true;
                args[1] = true;
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), firstRun, secondRun);

            queue.Run(queue.Define<Action<bool[]>>((args) =>
            {
                Assert.IsTrue(args[0] == false && args[1] == false);
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), firstRun, secondRun);
            bool done = await queue.Complete();
        }
        [Test()]
        public async Task CompleteObserverBeforeProcessingSecondOne()
        {
            bool firstRun = false;
            bool secondRun = false;

            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action<bool[]>>((args) =>
            {
                Assert.IsTrue(args[0] == false && args[1] == false);
                args[0] = true;
                args[1] = true;
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), firstRun, secondRun);
            
            await queue.Complete();
            firstRun = true;

            queue.Run(queue.Define<Action<bool[]>>((args) =>
            {
                Assert.IsTrue(args[0] == true && args[1] == false);
            }).OnCompleteCallback((object sender) =>
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), firstRun, secondRun);
            bool done = await queue.Complete();
        }
        
        /// <summary>
        /// Tasks were not finshed yet
        /// </summary>
        [Test()]
        public void TasksNotFinishedTest()
        {
            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action>(() => 
            { 
                Task.Delay(3000).Wait(); 
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
                Task.Delay(1000).Wait(); 
            }).OnCompleteCallback((object sender) => 
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }));

            Assert.IsTrue(queue.Tasks[0].IsCompleted == false &&
                queue.Tasks[1].IsCompleted == false &&
                queue.Tasks[2].IsCompleted == false);
        }
        
        [Test()]
        public async Task CancelFirstActionTest()
        {
            ITaskQueue queue = CreateTaskQueue();
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
            await queue.CancelAfter(2000);
        }

        [Test()]
        public async Task AddObjectToTheActions()
        {
            SharedObject objectsToShare1 = new SharedObject();
            SharedObject objectsToShare2 = new SharedObject();
            SharedObject objectsToShare3 = new SharedObject();
            SharedObject objectsToShare4 = new SharedObject();


            ITaskQueue queue = CreateTaskQueue();
            queue.Run(queue.Define<Action<SharedObject>>((sharedObject) => 
            {
                sharedObject.propertyOne = "value object 1";
            }).OnCompleteCallback((object sender) => 
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), objectsToShare1);

            queue.Run(queue.Define<Action<SharedObject[]>>((sharedObjects) => 
            {
                sharedObjects[1].propertyOne = "value object 2";
                Assert.AreEqual("value object 1", sharedObjects[0].propertyOne);
                Assert.AreEqual("value object 2", sharedObjects[1].propertyOne);
            }).OnCompleteCallback((object sender) => 
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), objectsToShare1,objectsToShare2);

            queue.Run(queue.Define<Action<SharedObject[]>>((sharedObjects) => 
            {
                sharedObjects[2].propertyOne = "value object 3";
                Assert.AreEqual("value object 1", sharedObjects[0].propertyOne);
                Assert.AreEqual("value object 2", sharedObjects[1].propertyOne);
                Assert.AreEqual("value object 3", sharedObjects[2].propertyOne);
            }).OnCompleteCallback((object sender) => 
            {
                Assert.AreEqual(ObserverStatus.Completed, (sender as IObserver).Status);
            }), objectsToShare1,objectsToShare2,objectsToShare3);

            bool done = await queue.Complete();
            Assert.AreEqual("value object 1", objectsToShare1.propertyOne);
            Assert.AreEqual("value object 2", objectsToShare2.propertyOne);
            Assert.AreEqual("value object 3", objectsToShare3.propertyOne);
        }

        [Test()]
        public async Task CancelSecondTaskTest()
        {
            ITaskQueue queue = CreateTaskQueue();
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
