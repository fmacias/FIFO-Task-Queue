# FifoTaskQueue

FifoTaskQueue is a FIFO task queue for .Net Core 3.1 and .Net Core 5.0 and it is based on .Net Standard 2.1.

The primary goal of this component is to run asynchronous processes into the given queue sequentially.

It is intended to be a loosely coupled component to be used within scalable applications. 
Interfaces and the concrete implementation are located in different Modules, one with the interfaces and the other with the concrete implementation.

It implements the Observer Pattern applied to the Tasks for Logging and possible monitoring issues.

Once a Task observation has been finalized, a callback to the Target object its Observer is sent.
This observer provides its processing state. Task are being disposed once the observation has been completed and is not available at the Callback of the observer. The Observer manages his own status, so that is not a problem.

The Task processor can be crated from the GUI Synchronization context to interact with the GUI Controls and from the current arbitrary Synchronization Context from which was created.

Each observed Task is subordinated to the previous one, so that, canceling one, these subordinated ones will also be canceled as long as the Actions manage the CancelationToken of the Queue.

It also observes the status of the processing Task for a tracking overview and supports task cancellation explicitly or after a given elapsed time.

Once a task has been cancelled of failed, these subordinated tasks will be
canceled before starting.

* Source Code is maintananced at the .Net Standard 2.1 project.
* Unit Test against .Net Core 3.1 and .Net Core 5.0 are provided.

# You will find

This Queue can be added to a GUI and interact properly with the controls because can be runnend in the same synchronization Context.

1. Task Cancelation and Task abortation with CancellationTokenSource and CancellationToken
2. Observer Design Pattern applied to the Tasks. It could be scaled for monitoring issues.
3. Event handlers.
4. IDisposable Pattern.
5. Test Cases
4. Inversion of Control

# Queue Creation

This component ist tightly coupled with NLog. I may remove this dependency in another version.

## TaskSheduler
The *TaskSheduler* associated with the main thread of the application
to interact with the GUI Controls or the one associated with the worker 
from with it was started.
```csharp
TaskScheduler currentWorkerSheduler = TaskShedulerWraper.Create().FromCurrentWorker();
````

## TasksProvider
The provider, which is the object that sends notifications to the observed Tasks.

Signatures:
```csharp
public static TasksProvider Create(ILogger logger)
public static TaskObserver Create(ILogger logger)
```

## FifoTaskQueue
Signature:
```csharp
public static FifoTaskQueue Create(TaskScheduler taskSheduler, ILogger logger)
```
Test Creation Extracted from Test
```csharp
...
    [TestFixture()]
    public class FifoTaskQueueTests
    {
        private FifoTaskQueue CreateTaskQueue()
        {
            return FifoTaskQueue.Create(
                TaskShedulerWraper.Create().FromCurrentWorker(),
                LogManager.GetCurrentClassLogger());
        }
...
```
# Usage
[Checkout some Use Cases at FifoTaskQueueTest](https://github.com/fmacias/ScalableComponents/blob/master/Components/FifoTaskQueue/netstandard2.1/Test/netcore5.0/FifoTaskQueue.Test/FifoTaskQueueTests.cs "FifoTaskQueueTest")

# Extracted from Test
```csharp
 [TestFixture()]
    public class FifoTaskQueueTests
    {
        public class SharedObject{
            public string propertyOne { get; set; }
        }

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
            using (ITaskQueue queue = CreateTaskQueue())
            {
                var observer2 = queue.Define<Action<int>>((args) => { });
                Assert.IsInstanceOf<IActionObserver<Action<int>>>(observer2);
                Assert.IsInstanceOf<IObserver>(observer2);
                Assert.IsInstanceOf<ITaskObserver>(observer2);
            }
        }
        [Test]
        public void test()
        {
            Action<bool> a = (boolValue) => { };
            Action<object> b = (booValue) => {
                a(true);
            };
            
        }
        [Test]
        public void RunTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
            {
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
        }

        [Test()]
        public void CompleteAtSyncMethodTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
            {
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
        }

        [Test()]
        public async Task VariablesNotPassedAsReferenceToTheActions()
        {
            bool firstRun = false;
            bool secondRun = false;
            
            using (ITaskQueue queue = CreateTaskQueue())
            {
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
        }
        [Test()]
        public async Task CompleteObserverBeforeProcessingSecondOne()
        {
            bool firstRun = false;
            bool secondRun = false;

            using (ITaskQueue queue = CreateTaskQueue())
            {
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
        }
        
        /// <summary>
        /// Tasks were not finshed yet
        /// </summary>
        [Test()]
        public void TasksNotFinishedTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
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
        
        [Test()]
        public async Task CancelFirstActionTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
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
        public async Task AddObjectToTheActions()
        {
            SharedObject objectsToShare1 = new SharedObject();
            SharedObject objectsToShare2 = new SharedObject();
            SharedObject objectsToShare3 = new SharedObject();
            SharedObject objectsToShare4 = new SharedObject();


            using (ITaskQueue queue = CreateTaskQueue())
            {
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
        }

        [Test()]
        public async Task CancelSecondTaskTest()
        {
            using (ITaskQueue queue = CreateTaskQueue())
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
```
> I am currently looking for a new Project. Please don't hesitate to contact me at fmaciasruano@gmail.com
