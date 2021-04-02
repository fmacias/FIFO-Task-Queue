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
5. Unit Tes with NUnit3. 

# Previsible enhancements to integrate this component into your project

In order to control the object instanciation and because this component does not reference any DI Container, I use to set the accesibility level of each constructor to private and leave the instanciation responsavility of the class to the class itself over a public static method called Create(params), sothat to integrate this component into another project, just the accesibility level of each constructor with the required DI metadata attributes are the unic previsible changes.

Any Logger and any implemtation based on any logger interface is integrated and I just write directly into the console. It could be another previsible change to integrate this component into your project.

# Queue Creation

The queue requires a *TaskSheduler* and a *TasksProvider*, given by composition and Injected at constructor.

## TaskSheduler
The *TaskSheduler* associated with the main thread of the application
to interact with the GUI Controls or the one associated with the worker 
from with it was started.
```csharp
TaskScheduler currentWorkerSheduler = TaskShedulerWraper.Create().FromCurrentWorker();
TaskScheduler currentGuiSheduler = TaskShedulerWraper.Create().FromGUIWorker();
````

## TasksProvider
The provider, which is the object that sends notifications to the observed Tasks.
```csharp
TasksProvider provider = TasksProvider.Create(new List<Task>()));
```

## FifoTaskQueue
The Queue
```csharp
FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider)
```
# Usage
[Checkout some Use Cases at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

# Example
## Simple usage
```csharp
 [Test()]
 public async Task AllTaskRemovedAfterCompletationOfEachObservationTest()
 {
     FifoTaskQueue queue = CreateTaskQueue();
     queue.Run(() => { });
     queue.Run(() => { });
     queue.Run(() => { });
     bool done = await queue.ObserveCompletation();
     Assert.IsTrue(queue.Tasks.Count() == 0);
     queue.Dispose();
 }
```
*Ouput:*
~~~
Task id: 1 Will be observe. State: RanToCompletion
Task id: 1 initial status RanToCompletion
Task id: 1 final status RanToCompletion
Task id: 4 Will be observe. State: WaitingToRun
Task id: 6 Will be observe. State: Running
Task id: 4 initial status RanToCompletion
Task id: 4 final status RanToCompletion
Task id: 6 initial status RanToCompletion
Task id: 6 final status RanToCompletion
Task 1 observation completed. Task Must be finished. Status:RanToCompletion 
Task 4 observation completed. Task Must be finished. Status:RanToCompletion 
Task 6 observation completed. Task Must be finished. Status:RanToCompletion 
All Queued Tasks have already been finalized!
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
     bool done = await queue.CancelAfter(2000, EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     Assert.IsTrue(queue.Tasks[0].IsFaulted, "First Task faulted");
     Assert.IsFalse(firstTaskFinished, "First Task's Action not terminated");
     Assert.IsTrue(queue.Tasks[1].IsCanceled, "Second Task Canceled");
     Assert.IsFalse(secondTaskfinished, "Second not finished");
     Assert.IsTrue(queue.Tasks[2].IsCanceled, "third Task Canceled");
     Assert.IsFalse(secondTaskfinished, "third task not finished");
     queue.Dispose();
}
```
*output*
~~~
Task id: 2 Will be observe. State: Running
Task id: 2 initial status Running
Task id: 3 Will be observe. State: WaitingForActivation
Task id: 5 Will be observe. State: WaitingForActivation
Task id: 3 initial status WaitingForActivation
Task id: 5 initial status WaitingForActivation
Task id: 5 Status transition to Canceled
Task id: 5 final status Canceled
Task id: 3 Status transition to Canceled
Task id: 3 final status Canceled
Task id: 2 Status transition to Faulted
Task id: 2 final status Faulted
Task 2 observation completed. Task Must be finished. Status:Faulted 
Task 3 observation completed. Task Must be finished. Status:Canceled 
Task 5 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
~~~
## Cancel after elapsed time
Does not break run execution because this provided task does not manage the ```CancelationToken```.
Cancelation was sent during the execution of the second task but it won't be aborted
because the action of the second task does not manage the cancelation Token of the queue,
so that, the second task will be finished and the next ones canceled.
```csharp
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
     await queue.CancelAfter(elapsedTimeToCancelQueue, EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     Assert.IsTrue(queue.Tasks[0].IsCompleted, "First Task Completed");
     Assert.IsTrue(queue.Tasks[1].IsCompleted && taskExecuted == true, "second Task Completed and executed");
     Assert.IsTrue(queue.Tasks[2].IsCanceled && queue.Tasks[3].IsCanceled, "Last tasks canceled");
     queue.ClearUpTasks();
     queue.Dispose();
}
```
*Output*
~~~
Task id: 53 Will be observe. State: WaitingToRun
Task id: 55 Will be observe. State: WaitingToRun
Task id: 53 initial status RanToCompletion
Task id: 53 final status RanToCompletion
Task id: 58 Will be observe. State: WaitingForActivation
Task id: 55 initial status Running
Task id: 60 Will be observe. State: WaitingForActivation
Task id: 58 initial status WaitingForActivation
Task id: 60 initial status WaitingForActivation
Task id: 60 Status transition to Canceled
Task id: 58 Status transition to Canceled
Task id: 60 final status Canceled
Task id: 58 final status Canceled
Task id: 55 Status transition to RanToCompletion
Task id: 55 final status RanToCompletion
Task 53 observation completed. Task Must be finished. Status:RanToCompletion 
Task 55 observation completed. Task Must be finished. Status:RanToCompletion 
Task 58 observation completed. Task Must be finished. Status:Canceled 
Task 60 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
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
     bool done = await queue.ObserveCompletation(EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     Assert.IsTrue(queue.Tasks[0].IsCompleted, "first task completed");
     Assert.IsTrue(queue.Tasks[1].IsCompleted, "second task completed");
     Assert.IsTrue(queue.Tasks[2].IsCompleted, "third task completed");
     Assert.AreEqual("a b c",String.Join(" ", objectRerenceToShare));
     queue.Dispose();
 }
```
*Output*
~~~
Task id: 160 Will be observe. State: WaitingToRun
Task id: 162 Will be observe. State: WaitingForActivation
Task id: 160 initial status Running
Task id: 160 Status transition to RanToCompletion
Task id: 160 final status RanToCompletion
Task id: 165 Will be observe. State: WaitingForActivation
Task id: 162 initial status Running
Task id: 162 Status transition to RanToCompletion
Task id: 162 final status RanToCompletion
Task id: 165 initial status RanToCompletion
Task id: 165 final status RanToCompletion
Task 160 observation completed. Task Must be finished. Status:RanToCompletion 
Task 162 observation completed. Task Must be finished. Status:RanToCompletion 
Task 165 observation completed. Task Must be finished. Status:RanToCompletion 
All Queued Tasks have already been finalized!
~~~
## Observe Tasks after each run.

In this example, after each task definition(Each Run), a queue process obervation or
a oberservation with a cancelation``` await queue.CancelAfter(2000, EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION); ```(see first run) will be invoked, forcing to process each task strictly sequentially. It is not necesary to do in that way, becasue task are bein managed by the
```Task.Factory```(StartNew and Continue), but it is usefull to do after a Run of a relly Long Task, for
example.
```csharp
 [Test()]
 public async Task CompleteTasks_Called_After_Each_TaskTest()
 {
     FifoTaskQueue queue = CreateTaskQueue();
     bool taskExecuted = false;
     int elapsedTimeToCancelQueue = 2000;
     queue.Run(() => {
         Task.Delay(5000, queue.CancellationToken).Wait();
     });
     await queue.CancelAfter(2000, EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     queue.Run(() => { });
         await queue.ObserveCompletation(EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     queue.Run(() => { });
         await queue.ObserveCompletation(EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     queue.Run(() => { });
     await queue.ObserveCompletation(EXCLUDE_TASK_CLEANUP_AFTER_FINALIZATION);
     Assert.IsTrue(queue.Tasks[0].IsFaulted, "First Task Faulted");
     Assert.IsTrue(queue.Tasks[1].IsCanceled, "second Task completed");
     Assert.IsTrue(queue.Tasks[2].IsCanceled && queue.Tasks[3].IsCanceled, "Last two completed");
     queue.ClearUpTasks();
     queue.Dispose();
 }
```
*Output*
Observing the task after each run.
~~~
Task id: 71 Will be observe. State: WaitingToRun
Task id: 71 initial status Running
Task id: 71 Status transition to Faulted
Task id: 71 final status Faulted
Task 71 observation completed. Task Must be finished. Status:Faulted 
All Queued Tasks have already been finalized!
Task id: 79 Will be observe. State: Canceled
Task id: 79 initial status Canceled
Task id: 79 final status Canceled
Task 79 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
Task id: 85 Will be observe. State: Canceled
Task id: 85 initial status Canceled
Task id: 85 final status Canceled
Task 85 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
Task id: 91 Will be observe. State: Canceled
Task id: 91 initial status Canceled
Task id: 91 final status Canceled
Task 91 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
~~~
[Checkout for more examples at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/DotNetCore/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

> I am currently looking for a new Project. Please don't hesitate to contact me at fmaciasruano@gmail.com
