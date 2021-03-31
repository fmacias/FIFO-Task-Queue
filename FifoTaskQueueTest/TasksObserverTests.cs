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
using System.Threading.Tasks;

namespace fmacias.Tests
{
    [TestFixture()]
    public class TasksObserverTests
    {
        [Test()]
        public void TasksObserverCreateTest()
        {
            Assert.IsTrue(TaskObserver.Create(Task.Run(() => { })) is IObserver<Task>);
        }

        [Test()]
        public void OnCompleted_NotCompletedTest()
        {
            Task.Run(async () =>
            {
                TaskObserver observer = TaskObserver.Create(Task.Run(() => {  }));
                observer.OnCompleted();
                bool completedTransition = await observer.TaskStatusCompletedTransition;
                Assert.IsFalse(completedTransition);
            });
            Task.Delay(1000).Wait();
        }
        [Test()]
        public void OnCompleted_CompletedTest()
        {
            Task.Run(async () =>
            {
                Task taskToObserve = Task.Run(() => { Task.Delay(2000).Wait(); });
                TaskObserver observer = TaskObserver.Create(taskToObserve).SetPollingStopElapsedTime(3000);
                observer.OnNext(taskToObserve);
                observer.OnCompleted();
                bool completedTransition = await observer.TaskStatusCompletedTransition;
                Assert.IsTrue(completedTransition);
            });
            Task.Delay(2500).Wait();
        }
    }
}