using EventAggregatorAbstract.Fmaciasruano.Components;
using NUnit.Framework;
using EventAggregatorComponents = EventAggregator.Fmaciasruano.Components;

namespace EventAggregator.Test
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
            IEventAggregator eventAggregator = EventAggregatorComponents.EventAggregator.Create(
                EventAggregatorComponents.ProcessEventFactory.Instance,
                EventAggregatorComponents.ProcessEventSubscriptorFactory.Instance, 
                EventAggregatorComponents.UIEventSubscriptorFactory.Instance);

            IProcessEvent trieggerObject = eventAggregator.EventFactory.Create<TestProcessEvent>();
            IProcessEventSubscriptor subscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            subscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(test_handler, trieggerObject);
            trieggerObject.Publish();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }

}