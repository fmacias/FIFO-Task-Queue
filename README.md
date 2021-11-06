# FifoTaskQueue

FifoTaskQueue is a FIFO task queue for .Net Core 3.1 and .Net Framework and it is able
to interact with the GUI controlls or to process Task at the backend based on the concept
of "first inpunt first output" (FIFO).

The primary goal of this component is to run asynchronous tasks sequentially,
assuming that each task is subordinated to the previous ones, so that, canceling
one, these subordinated wont be executed. 

It also observes the status of the processing Task for a tracking overview and
supports task cancellation explicitly or after a given elapsed time.
Once a task has been cancelled of failed, these subordinated tasks will be
canceled before starting.

*Source Code is maintananced at .Net Core Project. The source files of the .Net Framework project are just linkend to them for compilation.*

# You will find

This Queue can be added to a GUI and interact properly with the controls because can be runnend in the same synchronization Context.

1. Task Cancelation and Task abortation with CancellationTokenSource and CancellationToken
2. Observer Design Pattern applied to the Tasks. It could be scaled for monitoring issues.
3. Event handlers.
4. IDisposable Pattern.
5. Test Cases / Unit Tests with NUnit3. 

# Previsible enhancements to integrate this component into your project

In order to control the object instanciation and because this component does not reference any DI Container, I use to set the accesibility level of each constructor to private and leave the instanciation responsavility of the class to the class itself over a public static method called Create(params).

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
Visivility of this component has been set to internal and is not required at constructor anymore.
Signatures:
```csharp
public static TasksProvider Create(List<Task> tasks, ILogger logger)
public static TaskObserver Create(Task task, ILogger logger)
```

## FifoTaskQueue
Signature:
```csharp
public static FifoTaskQueue Create(TaskScheduler taskSheduler, ILogger logger)
```
# Usage
[Checkout some Use Cases at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

# Example
## Simple usage
```csharp
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
```
*Ouput:*
~~~
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 44 Will be observe. State: Running
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 44 initial status Running
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 45 Will be observe. State: WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 45 initial status WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 46 Will be observe. State: WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 46 initial status WaitingForActivation
2021-11-03 14:22:49.2871|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 45 Status transition to WaitingToRun
2021-11-03 14:22:49.2871|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 44 Status transition to RanToCompletion
2021-11-03 14:22:49.2871|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 44,  final status RanToCompletion, Duration: 507
2021-11-03 14:22:49.2871|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 44 observation completed successfully
2021-11-03 14:22:49.7891|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 45 Status transition to RanToCompletion
2021-11-03 14:22:49.7891|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 45,  final status RanToCompletion, Duration: 1009
2021-11-03 14:22:49.7891|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 45 observation completed successfully
2021-11-03 14:22:49.7891|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 46 Status transition to Running
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 46 Status transition to RanToCompletion
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 46,  final status RanToCompletion, Duration: 2020
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 46 observation completed successfully
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 44 unsubscribed!
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 45 unsubscribed!
2021-11-03 14:22:50.8012|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 46 unsubscribed!
~~~
## Simple Usage at async method
```csharp
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
```
*Ouput:*
~~~
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 41 Will be observe. State: WaitingToRun
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 41 initial status Running
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 42 Will be observe. State: WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43 Will be observe. State: WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 42 initial status WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 42 Status transition to WaitingToRun
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 41 Status transition to RanToCompletion
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 41,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 41 observation completed successfully
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43 initial status WaitingForActivation
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43 Status transition to WaitingToRun
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 42 Status transition to RanToCompletion
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43 Status transition to Running
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 42,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 42 observation completed successfully
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43 Status transition to RanToCompletion
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 43,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 43 observation completed successfully
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 41 unsubscribed!
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 42 unsubscribed!
2021-11-03 14:22:48.7771|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 43 unsubscribed!
~~~

## Cancel Task explicitly
Using the *CancelationToken* provided by the queue.

Cancelation will be sent during the execution of the first task.
During the execution of ```Task.Delay(5000, queue.CancellationToken).Wait();```.
As it manages the ```queue.CancellationToken```, this task will be aborted and the
subordinated ones canceled.
```csharp
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
```
*output*
~~~
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 23 Will be observe. State: WaitingToRun
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 23 initial status Running
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 24 Will be observe. State: WaitingForActivation
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 25 Will be observe. State: WaitingForActivation
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 24 initial status WaitingForActivation
2021-11-03 15:50:11.4979|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 25 initial status WaitingForActivation
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 24,  final status Canceled, Duration: 2055
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 25,  final status Canceled, Duration: 2053
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 23,  final status Faulted, Duration: 2056
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 23 observation completed successfully
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 24 observation completed successfully
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 25 observation completed successfully
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 23 unsubscribed!
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 24 unsubscribed!
2021-11-03 15:50:13.5540|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 25 unsubscribed!
~~~
## Cancel after elapsed time without manage the CancelationToken
Does not break run execution because this provided task does not manage the ```CancelationToken```.
Cancelation was sent during the execution of the second task but it won't be aborted
because the action of the second task does not manage the cancelation Token of the queue,
so that, the second task will be finished and the next ones canceled.
```csharp
        /// <summary>
        /// During the execution of the second task, a cancelation to the queue
        /// if the work takes longer than 2 seconds, has been sent.
        /// 
        /// But, as the task does not collect the cancelation token, the second task will be finished
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
```
*Output*
~~~
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 Will be observe. State: Running
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 initial status Running
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12 Status transition to RanToCompletion
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 12,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13 Will be observe. State: Running
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13 initial status Running
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14 Will be observe. State: WaitingForActivation
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15 Will be observe. State: WaitingForActivation
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14 initial status WaitingForActivation
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15 initial status WaitingForActivation
2021-11-03 14:22:30.4280|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 12 observation completed successfully
2021-11-03 14:22:32.4241|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 15,  final status Canceled, Duration: 2002
2021-11-03 14:22:32.4241|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 14,  final status Canceled, Duration: 2002
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13 Status transition to RanToCompletion
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 13,  final status RanToCompletion, Duration: 5000
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 13 observation completed successfully
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 14 observation completed successfully
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 15 observation completed successfully
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 12 unsubscribed!
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 13 unsubscribed!
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 14 unsubscribed!
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 15 unsubscribed!
~~~
## Cancel after elapsed time without managing the CancelationToken
```csharp
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
```
~~~
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 Will be observe. State: Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 initial status RanToCompletion
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Will be observe. State: Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 Will be observe. State: WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 initial status Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 Will be observe. State: WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 initial status WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 7 observation completed successfully
2021-11-03 14:22:28.4259|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 initial status WaitingForActivation
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9,  final status Canceled, Duration: 2006
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10,  final status Canceled, Duration: 1986
Task 11 Canceled.
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Status transition to RanToCompletion
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8,  final status RanToCompletion, Duration: 2008
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 8 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 9 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 10 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 7 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 8 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 9 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 10 unsubscribed!
~~~
## Share the same object into each task. 

It could be use to access sequentially GUI-Controls, and interact with them.

```csharp
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
```
*Output*
~~~
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 Will be observe. State: Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7 initial status RanToCompletion
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 7,  final status RanToCompletion, Duration: 0
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Will be observe. State: Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 Will be observe. State: WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 initial status Running
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 Will be observe. State: WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9 initial status WaitingForActivation
2021-11-03 14:22:28.4009|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 7 observation completed successfully
2021-11-03 14:22:28.4259|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10 initial status WaitingForActivation
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 9,  final status Canceled, Duration: 2006
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 10,  final status Canceled, Duration: 1986
Task 11 Canceled.
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8 Status transition to RanToCompletion
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 8,  final status RanToCompletion, Duration: 2008
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 8 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 9 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 10 observation completed successfully
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 7 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 8 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 9 unsubscribed!
2021-11-03 14:22:30.4110|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 10 unsubscribed!
~~~
## Observe Tasks after each run.

In this example, after each task definition(Each Run), a queue process obervation ```Continue()```or
a cancelation``` await queue.CancelAfter(2000); ```(see first run) will be invoked, forcing to process each task strictly sequentially. It is not necesary to do in that way, becasue task are bein managed by the
```Task.Factory```(StartNew and Continue), but it is usefull to do after a Run of a long Tasks, for
example.
```csharp
         /// <summary>
        /// It is posible to invoke the Complete method several times, for example after
        /// creation of the first run, so that the queue keep traking the first task as soon as it has 
        /// been created.
        /// 
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
```
*Output*
Observing the task after each run.
~~~
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 16 Will be observe. State: Running
2021-11-03 14:22:35.4213|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 16 initial status Running
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 16 Status transition to Faulted
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 16,  final status Faulted, Duration: 2010
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 16 observation completed successfully
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 16 unsubscribed!
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 17 Will be observe. State: Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 17 initial status Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 17,  final status Canceled, Duration: 0
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 17 observation completed successfully
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 17 unsubscribed!
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 18 Will be observe. State: Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 18 initial status Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 18,  final status Canceled, Duration: 0
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 18 observation completed successfully
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 18 unsubscribed!
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19 Will be observe. State: Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19 initial status Canceled
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task id: 19,  final status Canceled, Duration: 0
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Task 19 observation completed successfully
2021-11-03 14:22:37.4374|DEBUG|fmacias.Tests.FifoTaskQueueTests|Observer of Task 19 unsubscribed!
~~~
[Checkout for more examples at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

> I am currently looking for a new Project. Please don't hesitate to contact me at fmaciasruano@gmail.com
