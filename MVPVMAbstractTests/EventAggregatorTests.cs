using NUnit.Framework;
using MVPVMAbstract;
using System;
using System.Collections.Generic;
using System.Text;
using MVPVMAbstractTests;
using System.Reflection;

namespace MVPVMAbstract.Tests
{
    [TestFixture()]
    public class EventAggregatorTests
    {
        private bool processEventDone = false;
        private bool buttonSubscriptionOneDone = false;
        private bool buttonSubscriptionTwoDone = false;

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
        public void ResetEventsOutput()
        {
            processEventDone = false;
            buttonSubscriptionOneDone = false;
            buttonSubscriptionTwoDone = false;
        }

        [Test()]
        public void SubscribeTest()
        {
            IEventSubscriptable eventAggregator = new EventAggregator();
            IProcessEvent processEventObject = new TestProcessEvent();
            IEventSubscriptor processSubscriptor = new ProcessEventSubscriptor(eventAggregator, processEventObject);
            using (IEventUnsubscriber unsubscriber = eventAggregator.Subscribe(processSubscriptor))
            {
                processSubscriptor.AddEventHandler<ProcessEvent.ProcessEventHandler>(process_event_handler);
                processEventObject.Publish();
                Assert.AreEqual(true, processEventDone);
                Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            }

            Button btn = new Button();
            IEventSubscriptor uiSubscriptor = new UIEventSubscriptor(eventAggregator, btn, "Click");
            using (IEventUnsubscriber unsubscriber = eventAggregator.Subscribe(uiSubscriptor))
            {
                uiSubscriptor.AddEventHandler<Button.Handler>(button_event_handler);
                btn.OnClick();
                Assert.AreEqual(true, buttonSubscriptionOneDone);
                Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            }
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
        [Test()]
        public void GetEventSubscriptionsTest()
        {
            IEventSubscriptable eventAggregator = new EventAggregator();
            Button btn = new Button();
            IEventSubscriptor uiSubscriptor = new UIEventSubscriptor(eventAggregator, btn, "Click");
            uiSubscriptor.AddEventHandler<Button.Handler>(button_event_handler);
            IEventSubscriptor uiSubscriptor2 = new UIEventSubscriptor(eventAggregator, btn, "Click");
            uiSubscriptor2.AddEventHandler<Button.Handler>(button_event_handler_SecondSubscription);
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
            IEventSubscriptable eventAggregator = new EventAggregator();
            IProcessEvent processEventObject = new TestProcessEvent();
            IEventSubscriptor processSubscriptor = new ProcessEventSubscriptor(eventAggregator, processEventObject);
            processSubscriptor.AddEventHandler<ProcessEvent.ProcessEventHandler>(process_event_handler);
            processEventObject.Publish();
            Assert.AreEqual(true, processEventDone);
            Assert.AreEqual(1, eventAggregator.GetProcessEventSubscriptions(processEventObject).Count);
            processSubscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }

        [Test()]
        public void UnsubscribeAllTest()
        {
            IEventSubscriptable eventAggregator = new EventAggregator();
            IProcessEvent processEventObject = new TestProcessEvent();
            IEventSubscriptor processSubscriptor = new ProcessEventSubscriptor(eventAggregator, processEventObject);
            Button btn = new Button();
            IEventSubscriptor uiSubscriptor = new UIEventSubscriptor(eventAggregator, btn, "Click");
            eventAggregator.Subscribe(processSubscriptor);
            eventAggregator.Subscribe(uiSubscriptor);
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 2);
            eventAggregator.UnsubscribeAll();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }
}