﻿using NUnit.Framework;
using NSubstitute;
using System;

namespace SocialPoint.ScriptEvents
{
	internal struct TestEvent
	{
		public string Value;
	}


	[TestFixture]
	[Category("SocialPoint.ScriptEvents")]
	internal class EventDispatcherTests
	{

		EventDispatcher _dispatcher;
        TestEvent _testEvent;

		[SetUp]
		public void SetUp()
		{
			_dispatcher = new EventDispatcher();
            _testEvent = new TestEvent{ Value = "test" };
		}

		[Test]
		public void Raise_Calls_Listener()
		{
            string val = null;
			_dispatcher.AddListener<TestEvent>((ev) => {
                val = ev.Value;
			});


            _dispatcher.Raise(_testEvent);

            Assert.AreEqual(_testEvent.Value, val);
		}

        [Test]
        public void Raise_Calls_DefaultListener()
        {
            string val = null;
            _dispatcher.AddDefaultListener((ev) => {
                val = ((TestEvent)ev).Value;
            });

            _dispatcher.Raise(_testEvent);
            
            Assert.AreEqual(_testEvent.Value, val);
        }

        [Test]
        public void RemoveListener_Prevents_Call()
        {
            string val = null;
            Action<TestEvent> dlg = (ev) => {
                val = ev.Value;
            };

            _dispatcher.AddListener<TestEvent>(dlg);
            _dispatcher.RemoveListener(dlg);
            
            _dispatcher.Raise(_testEvent);
            
            Assert.AreEqual(null, val);
        }

        [Test]
        public void RemoveDefaultListener_Prevents_Call()
        {
            string val = null;
            Action<object> dlg = (ev) => {
                val = ((TestEvent)ev).Value;
            };
            
            _dispatcher.AddDefaultListener(dlg);
            _dispatcher.RemoveDefaultListener(dlg);
            
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

		[Test]
		public void Raise_Calls_ChildDispatcher()
		{
			var child = Substitute.For<IEventDispatcher>();
			_dispatcher.AddDispatcher(child);

			_dispatcher.Raise(_testEvent);

			child.Received().Raise(_testEvent);
		}
	}

}