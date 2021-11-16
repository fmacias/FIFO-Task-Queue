# FifoTaskQueue

FifoTaskQueue is a FIFO task queue for .Net Core 3.1 and .Net Core 5.0 and it is based on .Net Standard 2.1.

It is intended to be a loosely coupled component to be used within scalable applications. Interfaces and the concrete implementation are located in different Modules, one with the interfaces and the other with the concrete implementation.

It implements the Observer Pattern applied to the Tasks for Logging and possible monitoring issues.

Once a Task observation has been finalized, a callback to the Target object with the Observe its sent.
This observer provides also the processing state of the observer and access to its Task as well.

The Task processor can be crated from the GUI Synchronization context to interact with the GUI Controls and from the current arbitrary Synchronization Context from witch it was created.

The primary goal of this component is to run asynchronous processed into the given queue sequentially.

Each observed Task is subordinated to the previous one, so that, so that, canceling one, these subordinated ones will also be canceled as long as the Actions manage the CancelationToken of the Queue.

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
[Checkout some Use Cases at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

# Example
## Simple usage a a sync method
```csharp
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
```
*Ouput:*
~~~
2021-11-16 20:18:06.5870|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19 Will be observe. State: WaitingToRun
2021-11-16 20:18:06.5870|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 20 Will be observe. State: WaitingForActivation
2021-11-16 20:18:06.5870|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19 initial status Running
2021-11-16 20:18:06.5870|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 21 Will be observe. State: WaitingForActivation
2021-11-16 20:18:06.5870|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 20 initial status WaitingForActivation
2021-11-16 20:18:06.6190|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 21 initial status WaitingForActivation
2021-11-16 20:18:11.8173|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19 Status transition to RanToCompletion
2021-11-16 20:18:11.8173|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19,  final status RanToCompletion, Duration: 5228
2021-11-16 20:18:11.8173|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 19 observation completed successfully
2021-11-16 20:18:11.8173|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 20 Status transition to Running
2021-11-16 20:18:12.3334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 21 Status transition to Running
2021-11-16 20:18:12.3334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 20 Status transition to RanToCompletion
2021-11-16 20:18:12.3334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 20,  final status RanToCompletion, Duration: 5741
2021-11-16 20:18:12.3334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 20 observation completed successfully
2021-11-16 20:18:12.8334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 21 Status transition to RanToCompletion
2021-11-16 20:18:12.8334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 21,  final status RanToCompletion, Duration: 6213
2021-11-16 20:18:12.8334|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 21 observation completed successfully
~~~
## Simple Usage at async method
```csharp
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

```
*Ouput:*
~~~
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1 Will be observe. State: RanToCompletion
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1 initial status RanToCompletion
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 Will be observe. State: WaitingToRun
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 initial status Running
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 Status transition to RanToCompletion
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3 Will be observe. State: WaitingToRun
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3 initial status RanToCompletion
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 1 observation completed successfully
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 2 observation completed successfully
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 3 observation completed successfully
~~~

## Passing arguments to the actions(Action<object>)

```csharp
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
```
*output*
~~~
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1 Will be observe. State: RanToCompletion
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1 initial status RanToCompletion
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 1,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 Will be observe. State: WaitingToRun
2021-11-16 20:19:02.0672|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 initial status Running
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2 Status transition to RanToCompletion
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 2,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3 Will be observe. State: WaitingToRun
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3 initial status RanToCompletion
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 3,  final status RanToCompletion, Duration: 0
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 1 observation completed successfully
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 2 observation completed successfully
2021-11-16 20:19:02.0772|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 3 observation completed successfully
~~~
## Passing Variables as Arguments to the actions 
```csharp
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
```
*Output*
~~~
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 Will be observe. State: WaitingToRun
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Will be observe. State: WaitingForActivation
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 initial status Running
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 initial status WaitingForActivation
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 Will be observe. State: WaitingForActivation
2021-11-16 20:17:56.6165|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 initial status WaitingForActivation
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 Status transition to Running
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 Status transition to RanToCompletion
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9,  final status RanToCompletion, Duration: 5288
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 Status transition to RanToCompletion
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7,  final status RanToCompletion, Duration: 5312
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 7 observation completed successfully
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Status transition to RanToCompletion
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8,  final status RanToCompletion, Duration: 5312
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 8 observation completed successfully
2021-11-16 20:18:01.9058|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 9 observation completed successfully
~~~
## Cancel at first Action
```csharp
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
```
~~~
2021-11-16 20:18:01.9088|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 Will be observe. State: WaitingToRun
2021-11-16 20:18:01.9088|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 initial status Running
2021-11-16 20:18:01.9088|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 11 Will be observe. State: WaitingForActivation
2021-11-16 20:18:01.9088|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 Will be observe. State: WaitingForActivation
2021-11-16 20:18:01.9088|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 11 initial status WaitingForActivation
2021-11-16 20:18:02.0048|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 initial status WaitingForActivation
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 11,  final status Canceled, Duration: 2131
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 Status transition to Canceled
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12,  final status Canceled, Duration: 2036
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 Status transition to Faulted
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10,  final status Faulted, Duration: 2133
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 10 observation completed successfully
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 11 observation completed successfully
2021-11-16 20:18:04.0419|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 12 observation completed successfully
~~~
## Passing objects as Arguments to the actions

It could be use to access sequentially GUI-Controls, and interact with them.

```csharp
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
```
*Output*
~~~
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 4 Will be observe. State: WaitingToRun
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 5 Will be observe. State: WaitingForActivation
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 4 initial status RanToCompletion
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 4,  final status RanToCompletion, Duration: 0
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 5 initial status RanToCompletion
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 6 Will be observe. State: WaitingToRun
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 5,  final status RanToCompletion, Duration: 0
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 6 initial status RanToCompletion
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 6,  final status RanToCompletion, Duration: 0
2021-11-16 20:17:56.5905|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 6 observation completed successfully
~~~
## Cancel at second Task Test

```csharp
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
```
*Output*
~~~
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13 Will be observe. State: WaitingToRun
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14 Will be observe. State: WaitingForActivation
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13 initial status RanToCompletion
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13,  final status RanToCompletion, Duration: 0
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14 initial status Running
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15 Will be observe. State: WaitingForActivation
2021-11-16 20:18:04.0469|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15 initial status WaitingForActivation
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15 Status transition to Canceled
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15,  final status Canceled, Duration: 2025
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14 Status transition to Faulted
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14,  final status Faulted, Duration: 2025
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 14 observation completed successfully
2021-11-16 20:18:06.0730|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 15 observation completed successfully
~~~
[Checkout for more examples at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

> I am currently looking for a new Project. Please don't hesitate to contact me at fmaciasruano@gmail.com
