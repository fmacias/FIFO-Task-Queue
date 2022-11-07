using System;
using NUnit.Framework;
using EventAggregatorComponents = EventAggregator.Fmaciasruano.Components;
using EventAggregatorAbstract.Fmaciasruano.Components;
using EA = EventAggregator.Fmaciasruano.Components.EventAggregator;
using EventAggregator.Fmaciasruano.Components;

namespace EventAggregator.Test
{
    [TestFixture()]
    public class UIEventSubscriptorTests
    {
        private IEventAggregator eventAggregator;
        [SetUp]
        public void Initialize()
        {
            eventAggregator = EA.Create(
                ProcessEventFactory.Instance,
                ProcessEventSubscriptorFactory.Instance,
                UIEventSubscriptorFactory.Instance);
        }
        private void test_Button_handler(object sender)
        {
            Assert.IsInstanceOf<Button>(sender);
        }
        [Test()]
        public void UIEventSubscriptorTest()
        {
            Button trieggerObject = new Button();
            IUIEventSubscriptor subscriptor = UIEventSubscriptor.Create(eventAggregator);
            subscriptor.AddEventHandler<Button.Handler>(test_Button_handler, "Click", trieggerObject);
            trieggerObject.OnClick();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }
}