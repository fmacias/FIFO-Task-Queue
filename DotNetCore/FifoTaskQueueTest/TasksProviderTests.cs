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
using NLog;
using Moq;

namespace fmacias.Tests
{
    [TestFixture()]
    public class TasksProviderTests
    {
        private ILogger GetLogger()
        {
            Mock<ILogger> logger = new Mock<ILogger>();
            logger.Setup(p => p.Info(It.IsAny<string>()));
            logger.Setup(p => p.Debug(It.IsAny<string>()));
            logger.Setup(p => p.Warn(It.IsAny<string>()));
            logger.Setup(p => p.Error(It.IsAny<string>()));
            return logger.Object;
        }
        [Test()]
        public void TasksProviderTest()
        {
            Assert.IsTrue(TasksProvider.Create(new List<Task>(),GetLogger()) is IObservable<Task>);
        }

        [Test()]
        public void SubscribeTest()
        {
            List<Task> tasks = new List<Task>();
            Task task = Task.Run(() => { });
            tasks.Add(task);
            TasksProvider provider = TasksProvider.Create(new List<Task>(),GetLogger());
            provider.AddTask(task);
            TaskObserver observer = TaskObserver.Create(task,GetLogger());
            observer.OnNext(task);
            IDisposable unsubscriber = provider.Subscribe(observer);
            Assert.IsTrue(provider.Subscribe(observer) is IDisposable);
        }

        [Test()]
        public void AddTaskTest()
        {
            TasksProvider provider = TasksProvider.Create(new List<Task>(),GetLogger());
            provider.AddTask(Task.Run(() => { }));
            Assert.IsTrue(provider.Tasks.Count == 1);
        }
    }
}