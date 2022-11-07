using EventAggregatorAbstract.Fmaciasruano.Components;
using NUnit.Framework;
using EventAggregatorComponents=EventAggregator.Fmaciasruano.Components;

namespace EventAggregator.Test
{
    [TestFixture()]
    public class EventAggregatorTests
    {
        private bool processEventDone = false;
        private bool buttonSubscriptionOneDone = false;
        private bool buttonSubscriptionTwoDone = false;
        private IEventAggregator eventAggregator;
        private void process_event_handler(object sender)
        {
            processEventDone = true;
            Assert.IsInstanceOf<TestProcessEvent>(sender);
        }
        private void button_event_handler(object sender)
        {
            buttonSubscriptionOneDone = true;
            Assert.IsInstanceOf<Button>(sender);
        }
        private void button_event_handler_SecondSubscription(object sender)
        {
            buttonSubscriptionTwoDone = true;
            Assert.IsInstanceOf<Button>(sender);
        }
        [SetUp]
        public void Reset()
        {
            eventAggregator = EventAggregatorComponents.EventAggregator.Create(
                EventAggregatorComponents.ProcessEventFactory.Instance,
                EventAggregatorComponents.ProcessEventSubscriptorFactory.Instance, 
                EventAggregatorComponents.UIEventSubscriptorFactory.Instance);
            processEventDone = false;
            buttonSubscriptionOneDone = false;
            buttonSubscriptionTwoDone = false;
        }

        [Test()]
        public void SubscribeTest()
        {
            IProcessEvent processEventObject = eventAggregator.EventFactory.Create<TestProcessEvent>();
            IProcessEventSubscriptor processSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            using (IEventUnsubscriber unsubscriber = eventAggregator.Subscribe(processSubscriptor))
            {
                processSubscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(process_event_handler, processEventObject);
                processEventObject.Publish();
                Assert.AreEqual(true, processEventDone);
                Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            }

            Button btn = new Button();
            IUIEventSubscriptor uiSubscriptor = eventAggregator.UIEventSubscriptorFactory.Create(eventAggregator);
            using (IEventUnsubscriber unsubscriber = eventAggregator.Subscribe(uiSubscriptor))
            {
                uiSubscriptor.AddEventHandler<Button.Handler>(button_event_handler, "Click", btn);
                btn.OnClick();
                Assert.AreEqual(true, buttonSubscriptionOneDone);
                Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            }
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
        [Test()]
        public void GetEventSubscriptionsTest()
        {
            Button btn = new Button();
            IUIEventSubscriptor uiSubscriptor = eventAggregator.UIEventSubscriptorFactory.Create(eventAggregator);
            uiSubscriptor.AddEventHandler<Button.Handler>(button_event_handler, "Click", btn);
            IUIEventSubscriptor uiSubscriptor2 = eventAggregator.UIEventSubscriptorFactory.Create(eventAggregator);
            uiSubscriptor2.AddEventHandler<Button.Handler>(button_event_handler_SecondSubscription, "Click", btn);
            Assert.AreEqual(2, eventAggregator.GetEventSubscriptions(btn, "Click").Count, "2 Subscriptions for Button Object");
            btn.OnClick();
            Assert.AreEqual(true, buttonSubscriptionOneDone);
            Assert.AreEqual(true, buttonSubscriptionTwoDone);
            uiSubscriptor.Unsubscribe();
            uiSubscriptor2.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }

        [Test()]
        public void GetProcessEventSubscriptionsTest()
        {
            IProcessEvent processEventObject = eventAggregator.EventFactory.Create<TestProcessEvent>();
            IProcessEventSubscriptor processSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            processSubscriptor.AddEventHandler<IProcessEvent.ProcessEventHandler>(process_event_handler, processEventObject);
            processEventObject.Publish();
            Assert.AreEqual(true, processEventDone);
            Assert.AreEqual(1, eventAggregator.GetProcessEventSubscriptions(processEventObject).Count);
            processSubscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }

        [Test()]
        public void UnsubscribeAllTest()
        {
            IProcessEvent processEventObject = eventAggregator.EventFactory.Create<TestProcessEvent>();
            IEventSubscriptor processSubscriptor = eventAggregator.EventSubscriptorFactory.Create(eventAggregator);
            Button btn = new Button();
            IEventSubscriptor uiSubscriptor = eventAggregator.UIEventSubscriptorFactory.Create(eventAggregator);
            eventAggregator.Subscribe(processSubscriptor);
            eventAggregator.Subscribe(uiSubscriptor);
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 2);
            eventAggregator.UnsubscribeAll();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }
}