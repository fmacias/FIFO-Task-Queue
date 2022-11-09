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
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using Moq;
using NLog;
using NUnit.Framework;


namespace FifoTaskQueueNC5_0.Test.Fmaciasruano.Components
{
    [TestFixture()]
    public class TasksObserverTests
    {
        private IActionObserver<Action> taskObserver;
        [SetUp]
        public void ResetEventsOutput()
        {
            taskObserver = TaskObserver<Action>.Create(GetLogger());
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
    }
}