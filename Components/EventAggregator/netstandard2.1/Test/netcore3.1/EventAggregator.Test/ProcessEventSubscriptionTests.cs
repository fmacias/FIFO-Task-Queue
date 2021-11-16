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
            IEventSubscriptable eventAggregator = new EventAggregator(new ProcessEventFactory(), new ProcessEventSubscriptorFactory(), new UIEventSubscriptorFactory());
            IProcessEvent trieggerObject = eventAggregator.ProcessEventFactory.Create<TestProcessEvent>();
            IProcessEventSubscriptor subscriptor = new ProcessEventSubscriptor(eventAggregator);
            subscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(test_handler, trieggerObject);
            trieggerObject.Publish();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }

}