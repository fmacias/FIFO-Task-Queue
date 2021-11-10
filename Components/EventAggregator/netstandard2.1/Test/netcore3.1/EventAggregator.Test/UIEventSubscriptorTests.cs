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
            IEventSubscriptable eventAgregator = new EventAggregator();
            Button trieggerObject = new Button();
            IEventSubscriptor subscriptor = new UIEventSubscriptor(eventAgregator,trieggerObject, "Click");
            subscriptor.AddEventHandler<Button.Handler>(test_Button_handler);
            trieggerObject.OnClick();
            Assert.IsTrue(eventAgregator.Subscriptions.Count == 1);
            subscriptor.Unsubscribe();
            Assert.IsTrue(eventAgregator.Subscriptions.Count == 0);
        }
    }
}