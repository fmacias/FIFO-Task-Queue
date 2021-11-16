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
    public class UIEventSubscriptorTests
    {
        private void test_Button_handler(object sender)
        {
            Assert.IsInstanceOf<Button>(sender);
        }
        [Test()]
        public void UIEventSubscriptorTest()
        {
            IEventSubscriptable eventAggregator = new EventAggregator(new ProcessEventFactory(), new ProcessEventSubscriptorFactory(), new UIEventSubscriptorFactory());
            Button trieggerObject = new Button();
            IUIEventSubscriptor subscriptor = new UIEventSubscriptor(eventAggregator);
            subscriptor.AddEventHandler<Button.Handler>(test_Button_handler, "Click", trieggerObject);
            trieggerObject.OnClick();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAggregator.Subscriptions.Count == 0);
        }
    }
}