using NUnit.Framework;
using NSubstitute;
using System;

namespace SocialPoint.ScriptEvents
{
    [TestFixture]
    [Category("SocialPoint.ScriptEvents")]
    class EventDispatcherTests : BaseScriptEventsTests
    {
        [SetUp]
        public void SetUpBase()
        {
            SetUp();
        }

        [Test]
        public void Raise_Calls_Listener()
        {
            string val = null;
            _dispatcher.AddListener<TestEvent>(ev => {
                val = ev.Value;
            });
            _dispatcher.Raise(new OtherTestEvent{ Value = 2 });
            Assert.IsNull(val);
            _dispatcher.Raise(_testEvent);
            Assert.AreEqual(_testEvent.Value, val);
        }

        [Test]
        public void Connect_Works()
        {
            _dispatcher.Connect<OtherTestEvent, TestEvent>(ev => new TestEvent {
                Value = ev.Value.ToString()
            });
            string val = null;
            _dispatcher.AddListener<TestEvent>(ev => {
                val = ev.Value;
            });
            _dispatcher.Raise(new OtherTestEvent{ Value = 2 });
			
            Assert.AreEqual("2", val);
        }

        [Test]
        public void Raise_Calls_DefaultListener()
        {
            string val = null;
            _dispatcher.AddDefaultListener(ev => {
                val = ((TestEvent)ev).Value;
            });

            _dispatcher.Raise(_testEvent);
            
            Assert.AreEqual(_testEvent.Value, val);
        }

        [Test]
        public void RemoveListener_Prevents_Call()
        {
            string val = null;
            Action<TestEvent> dlg = ev => {
                val = ev.Value;
            };

            _dispatcher.AddListener<TestEvent>(dlg);
            Assert.IsTrue(_dispatcher.RemoveListener(dlg));
            
            _dispatcher.Raise(_testEvent);
            
            Assert.AreEqual(null, val);
        }

        [Test]
        public void RemoveDefaultListener_Prevents_Call()
        {
            string val = null;
            Action<object> dlg = ev => {
                val = ((TestEvent)ev).Value;
            };
            
            _dispatcher.AddDefaultListener(dlg);
            Assert.IsTrue(_dispatcher.RemoveDefaultListener(dlg));
            
            _dispatcher.Raise(_testEvent);
            
            Assert.AreEqual(null, val);
        }

        [Test]
        public void Bridge_Load_Called()
        {
            var bridge = Substitute.For<IEventsBridge>();
            
            _dispatcher.AddBridge(bridge);
            _dispatcher.AddBridge(bridge);
            
            bridge.Received(1).Load(_dispatcher);
        }

        [Test]
        public void Bridge_Dispose_Called()
        {
            var bridge = Substitute.For<IEventsBridge>();
            
            _dispatcher.AddBridge(bridge);
            _dispatcher.Dispose();
            
            bridge.Received().Dispose();
        }

    }

}