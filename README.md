# FifoTaskQueue

FifoTaskQueue is a FIFO task queue for .Net Framework. and it is able
to interact with the GUI controlls or to process Task at the backend.

The primary goal of this component is to run asynchronous tasks sequentially,
assuming that each task is subordinated to the previous ones, so that, canceling
one, these subordinated wont be executed. 

It also observes the status of the processing Task for a tracking overview and
supports task cancellation explicitly or after a given elapsed time.
Once a task has been cancelled of failed, these subordinated tasks will be
canceled before starting properly.

# You will find

This Queue can be added to a GUI and interact properly with the controls because can be runnend in the same synchronization Context.

1. Cancel concatenated Task using CancellationTokenSource and CancellationToken
2. NUnit test provided. 

# Queue Creation

The requires a *TaskSheduler* and a *TasksProvider*.

## TaskSheduler
The *TaskSheduler* associated with the main thread of the application
to interact with the GUI Controls or the one associated with the worker 
from with it was started.
```[C#]
TaskScheduler currentWorkerSheduler = TaskShedulerWraper.Create().FromCurrentWorker();
TaskScheduler currentGuiSheduler = TaskShedulerWraper.Create().FromGUIWorker();
````

## TasksProvider
The provider, which is the object that sends notifications to the observed Tasks.
```[C#]
TasksProvider provider = TasksProvider.Create(new List<Task>()));
```

## FifoTaskQueue
```[C#]
FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider)
```
# Usage
[Checkout some Use Cases at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")
# Example
## Simple usage
```[C#]
FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider);
queue.Run(() => { });
queue.Run(() => { });
queue.Run(() => { });
bool done = await queue.Complete();
queue.Dispose();
```
*Ouput:*
~~~
Task id: 11 Will be observe. State: Running
Task id: 12 Will be observe. State: WaitingForActivation
Task id: 11 initial status Running
Task id: 11 final status RanToCompletion
Task id: 15 Will be observe. State: WaitingForActivation
Task id: 12 initial status Running
Task id: 12 Status transition to RanToCompletion
Task id: 12 final status RanToCompletion
Task id: 15 initial status RanToCompletion
Task id: 15 final status RanToCompletion
Task 11 observation completed. Task Must be finished. Status:RanToCompletion 
Task 12 observation completed. Task Must be finished. Status:RanToCompletion 
Task 15 observation completed. Task Must be finished. Status:RanToCompletion 
All Queued Tasks have already been finalized!
~~~
## Cancel Task explicitly
Using the *CancelationToken* provided by the queue.

```[C#]
 FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider);
 queue.Run(() =>
 {
	queue.CancelExecution();
	Task.Delay(5000, queue.CancellationToken).Wait();
});
queue.Run(() =>
{
	secondTaskfinished = true;
});
queue.Run(() =>
{
	thirdTaskStarted = false;
});
bool done = await queue.Complete();
queue.Dispose();
```
~~~
Task id: 92 Will be observe. State: Running
Task id: 92 initial status Faulted
Task id: 92 final status Faulted
Task id: 94 Will be observe. State: Canceled
Task id: 94 initial status Canceled
Task id: 94 final status Canceled
Task id: 96 Will be observe. State: Canceled
Task id: 96 initial status Canceled
Task id: 96 final status Canceled
Task 92 observation completed. Task Must be finished. Status:Faulted 
Task 94 observation completed. Task Must be finished. Status:Canceled 
Task 96 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
~~~
## Cancel during execution and break method completation

```[C#]
FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider);
queue.Run(() => { });
queue.Run(() => {
	Task.Delay(5000, queue.CancellationToken).Wait();
});
queue.Run(() => { });
queue.Run(() => { });
int elapsedTimeToCancelQueue = 2000;
await queue.Complete(elapsedTimeToCancelQueue);
queue.Dispose();
```
*Output*
~~~
Task id: 31 Will be observe. State: WaitingToRun
Task id: 33 Will be observe. State: WaitingForActivation
Task id: 31 initial status Running
Task id: 31 final status RanToCompletion
Task id: 33 initial status Running
Task id: 36 Will be observe. State: WaitingForActivation
Task id: 38 Will be observe. State: WaitingForActivation
Task id: 36 initial status WaitingForActivation
Task id: 38 initial status WaitingForActivation
Task id: 38 Status transition to Canceled
Task id: 38 final status Canceled
Task id: 33 Status transition to Faulted
Task id: 33 final status Faulted
Task id: 36 Status transition to Canceled
Task id: 36 final status Canceled
Task 31 observation completed. Task Must be finished. Status:RanToCompletion 
Task 33 observation completed. Task Must be finished. Status:Faulted 
Task 36 observation completed. Task Must be finished. Status:Canceled 
Task 38 observation completed. Task Must be finished. Status:Canceled 
All Queued Tasks have already been finalized!
~~~

## Share the same object into each task. It could als be a GUI-Control, for example.

In this example I also comment the invokation to the Complete() Method,
which starts the completation of each TaskObserver between others, to show that it
can be invoked many times but it is not neccesary.

On disposing, each instance of TaskObsever will finished and the tasks will
be disposed.
```[C#]

object[] objectRerenceToShare = new object[3];
FifoTaskQueue queue = FifoTaskQueue.Create(currentGuiSheduler,provider);
queue.Run((sharedObject) =>
{
	((object[])sharedObject)[0] = "a";
}, objectRerenceToShare);
//
//bool done = await queue.Complete();
queue.Run((sharedObject) =>
{
	((object[])sharedObject)[1] = "b";
}, objectRerenceToShare);
//bool done = await queue.Complete();
queue.Run((sharedObject) =>
{
	((object[])sharedObject)[2] = "c";
}, objectRerenceToShare);
//
//bool done = await queue.Complete();
queue.Dispose();
```

[Checkout for more examples at FifoTaskQueueTest](https://github.com/fmacias/FIFO-Task-Queue/blob/master/FifoTaskQueueTest/FifoTaskQueueTests.cs "FifoTaskQueueTest")

> I am currently looking for a new Project. Please don't hesitate to contact me at fmaciasruano@gmail.com
