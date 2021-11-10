using NUnit.Framework;
using MVPVMAbstract;
using System;
using System.Collections.Generic;
using System.Text;
using MVPVMAbstractTests;
using fmacias.Components.EventAggregator;

namespace MVPVMAbstract.Tests
{
    [TestFixture()]
    public class ProcessEventSubscriptionTests
    {
        private void test_handler(object sender)
        {
            Assert.IsInstanceOf<TestProcessEvent>(sender);
        }
        [Test()]
        public void ProcessEventSubscriptionTest()
        {
            IEventSubscriptable eventAgregator = new EventAggregator();
            TestProcessEvent trieggerObject = new TestProcessEvent();
            IEventSubscriptor subscriptor = new ProcessEventSubscriptor(eventAgregator,trieggerObject);
            subscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(test_handler);
            trieggerObject.Publish();
            Assert.IsTrue(eventAgregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAgregator.Subscriptions.Count == 0);
        }
    }

}