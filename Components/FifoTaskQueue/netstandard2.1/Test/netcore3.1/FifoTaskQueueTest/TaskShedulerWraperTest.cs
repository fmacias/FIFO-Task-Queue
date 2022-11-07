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
using System.Threading;
using System.Threading.Tasks;
using FifoTaskQueue;
using FifoTaskQueue.Fmaciasruano.Components;
using fmacias.Components.FifoTaskQueue;

namespace fmacias.Components.FifoTaskQueue.Tests
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
            Sheduler synchronizationContextSheduler = Sheduler.Create();
            TaskScheduler taskSheduler = synchronizationContextSheduler.FromCurrentWorker();
            Assert.IsNotNull(taskSheduler);
        }
        /// <summary>
        /// If instances exist, they will always be the same object.
        /// </summary>
        [Test()]
        public void FromCurrentWorker_instancesAreTheSameTest()
        {
            Sheduler synchronizationContextSheduler = Sheduler.Create();
            TaskScheduler taskSheduler1 = synchronizationContextSheduler.FromCurrentWorker();
            TaskScheduler taskSheduler2 = synchronizationContextSheduler.FromCurrentWorker();
            Assert.IsTrue(Object.ReferenceEquals(taskSheduler1, taskSheduler2));
            Assert.IsTrue(Object.ReferenceEquals(synchronizationContextSheduler.Sheduler, taskSheduler1));
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
            Sheduler synchronizationContextSheduler = Sheduler.Create();
            Assert.Throws(typeof(System.InvalidOperationException), delegate { synchronizationContextSheduler.FromGUIWorker(); });
        }

        /// <summary>
        /// To simulate such a situation, to perform the action from the GUI Worker,
        /// i need create an set a new <see cref="SynchronizationContext"/>;
        /// </summary>
        [Test()]
        public void FromGUIWorkerTest()
        {
            SynchronizationContext synchronizationContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            Sheduler synchronizationContextSheduler = Sheduler.Create();
            Assert.IsInstanceOf(typeof(TaskScheduler), synchronizationContextSheduler.FromGUIWorker());
        }
    }
}
