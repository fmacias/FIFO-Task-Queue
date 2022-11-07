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

using System;
using System.Threading.Tasks;
using EventAggregator.Fmaciasruano.Components;
using EventAggregatorAbstract.Fmaciasruano.Components;
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using Moq;
using NLog;
using NUnit.Framework;
using EA = EventAggregator.Fmaciasruano.Components.EventAggregator;


namespace FifoTaskQueueNC5_0.Test.Fmaciasruano.Components
{
    [TestFixture()]
    public class TasksObserverTests
    {
        private IEventAggregator eventAggregator;
        private IActionObserver<Action> taskObserver;
        [SetUp]
        public void ResetEventsOutput()
        {
            eventAggregator = EA.Create(
                ProcessEventFactory.Instance,
                ProcessEventSubscriptorFactory.Instance,
                UIEventSubscriptorFactory.Instance);

            taskObserver = TaskObserver<Action>.Create(eventAggregator, GetLogger());
        }
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
        public void TasksObserverCreateTest()
        {
            Assert.IsInstanceOf<IActionObserver<Action>>(taskObserver);
        }
        [Test()]
        public async Task OnCompleted_NotCompletedTest()
        {
            throw new NotImplementedException();
            //bool completedTransition = await taskObserver.TaskStatusCompletedTransition;
            //Assert.IsFalse(completedTransition);
        }
        [Test()]
        public async Task OnCompleted_CompletedTest()
        {
            throw new NotImplementedException();
            /*
            Task task = Task.Run(() => { Task.Delay(2000).Wait(); });

            taskObserver.OnNext(task);
            bool completedTransition = await taskObserver.TaskStatusCompletedTransition;
            Assert.IsTrue(completedTransition);*/
        }
    }
}