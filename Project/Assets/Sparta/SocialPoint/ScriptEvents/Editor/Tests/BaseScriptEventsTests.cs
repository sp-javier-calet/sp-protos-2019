﻿using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    struct TestEvent
    {
        public string Value;
    }

    struct OtherTestEvent
    {
        public int Value;
    }

    class TestEventConverter : BaseScriptEventConverter<TestEvent>
    {
        public TestEventConverter() : base("test")
        {
        }

        override protected TestEvent ParseEvent(Attr data)
        {
            return new TestEvent {
                Value = data.AsValue.ToString()
            };
        }

        override protected Attr SerializeEvent(TestEvent ev)
        {
            return new AttrString(ev.Value);
        }
    }

    class OtherTestEventConverter : BaseScriptEventConverter<OtherTestEvent>
    {
        public OtherTestEventConverter() : base("other")
        {
        }

        override protected OtherTestEvent ParseEvent(Attr data)
        {
            return new OtherTestEvent {
                Value = data.AsValue.ToInt()
            };
        }

        override protected Attr SerializeEvent(OtherTestEvent ev)
        {
            return new AttrString(ev.Value.ToString());
        }
    }


    class BaseScriptEventsTests
    {
        protected EventDispatcher _dispatcher;
        protected ScriptEventProcessor _scriptProcessor;
        protected TestEvent _testEvent;
        protected Attr _testArgs;


        virtual public void SetUp()
        {
            _dispatcher = new EventDispatcher();
            _scriptProcessor = new ScriptEventProcessor(_dispatcher);
            var testConv = new TestEventConverter();
            _scriptProcessor.RegisterConverter(testConv);
            _scriptProcessor.RegisterConverter(new OtherTestEventConverter());
            _testEvent = new TestEvent{ Value = "test_value" };
            _testArgs = testConv.Serialize(_testEvent);
        }
    }

}