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
using System.Threading;
using System.Threading.Tasks;
using FifoTaskQueue.Fmaciasruano.Components;
using NUnit.Framework;

namespace FifoTaskQueueNC5_0.Test.Fmaciasruano.Components
{
    [TestFixture()]
    class TaskShedulerWraperTest
    {
        /// <summary>
        /// 
        /// </summary>
        [Test()]
        public void FromCurrentWorkerTest()
        {
            TaskScheduler taskScheduler = Sheduler.FromCurrentWorker();
            Assert.IsNotNull(taskScheduler);
        }
        /// <summary>
        /// If instances exist, they will always be the same object.
        /// </summary>
        [Test()]
        public void FromCurrentWorker_instancesAreTheSameTest()
        {
            TaskScheduler taskSheduler1 = Sheduler.FromCurrentWorker();
            TaskScheduler taskSheduler2 = Sheduler.FromCurrentWorker();
            Assert.IsTrue(Object.ReferenceEquals(taskSheduler1, taskSheduler2));
        }

        /// <summary>
        /// Most GUI applications for the .Net Framwork let you access GUI Objects only from the Thread
        /// that creates and manages the UI.
        /// Whenn you try to access thos UI Elements from another thread, a <see cref="System.InvalidOperationException"/>
        /// is thrown.
        /// </summary>
        [Test()]
        public void FromGUIWorker_instancesAreTheSameTest()
        {
            SynchronizationContext synchronizationContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            Assert.IsInstanceOf<TaskScheduler>(Sheduler.FromGuiWorker());
        }
    }
}
