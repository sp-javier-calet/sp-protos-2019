using NSubstitute;
using NUnit.Framework;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    [TestFixture]
    [Category("SocialPoint.ScriptEvents")]
    class ScriptEventDispatcherTests : BaseScriptEventsTests
    {
        [SetUp]
        public void SetUpBase()
        {
            SetUp();
        }

        [Test]
        public void Script_Process_Calls_Listener()
        {
            string val = null;
            _processor.RegisterHandler<TestEvent>(ev => {
                val = ev.Value;
            });
            _scriptProcessor.Process("test", _testArgs);
            Assert.AreEqual(_testEvent.Value, val);
        }

        [Test]
        public void Script_Process_Calls_Script_Listener()
        {
            Attr val = null;
            _scriptProcessor.RegisterHandler("test", args => {
                val = (Attr)args.Clone();
            });
            _scriptProcessor.Process("test", _testArgs);
            Assert.AreEqual(_testArgs, val);
        }

        [Test]
        public void Process_Calls_Script_Listener()
        {
            Attr val = null;
            _scriptProcessor.RegisterHandler("test", args => {
                val = (Attr)args.Clone();
            });
            _processor.Process(_testEvent);
            Assert.AreEqual(_testEvent.Value, val.ToString());
        }

        [Test]
        public void Bridge_Load_Called()
        {
            var bridge = Substitute.For<IScriptEventsBridge>();
			
            _scriptProcessor.RegisterBridge(bridge);
            _scriptProcessor.RegisterBridge(bridge);
			
            bridge.Received(1).Load(_scriptProcessor, _processor);
        }

        [Test]
        public void Bridge_Dispose_Called()
        {
            var bridge = Substitute.For<IScriptEventsBridge>();
			
            _scriptProcessor.RegisterBridge(bridge);
            _scriptProcessor.Dispose();
			
            bridge.Received().Dispose();
        }

        [Test]
        public void NameCondition_Works()
        {
            string evName = null;
            var cond = new NameCondition("te*");
            _scriptProcessor.RegisterHandler(cond, (name, args) => {
                evName = name;
            });

            _processor.Process(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _processor.Process(new TestEvent{ Value = "lala" });
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void ArgumentCondition_Works()
        {
            string evName = null;
            var cond = new ArgumentsCondition(_testArgs);
            _scriptProcessor.RegisterHandler(cond, (name, args) => {
                evName = name;
            });
			
            _processor.Process(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _processor.Process(new TestEvent{ Value = "lala" });
            Assert.IsNull(evName);
            _processor.Process(_testEvent);
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void NotCondition_Works()
        {
            string evName = null;
            var cond = new NotCondition(new ArgumentsCondition(_testArgs));
            _scriptProcessor.RegisterHandler(cond, (name, args) => {
                evName = name;
            });
			
            _processor.Process(_testEvent);
            Assert.IsNull(evName);
            _processor.Process(new TestEvent{ Value = "lala" });
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void AndCondition_Works()
        {
            string evName = null;
            var cond = new AndCondition(new IScriptCondition[] {
                new ArgumentsCondition(new AttrString("1")),
                new NameCondition("??st")
            });
            _scriptProcessor.RegisterHandler(cond, (name, args) => {
                evName = name;
            });
			
            _processor.Process(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _processor.Process(_testEvent);
            Assert.IsNull(evName);
            _processor.Process(new TestEvent{ Value = "1" });
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void OrCondition_Works()
        {
            string evName = null;
            var cond = new OrCondition(new IScriptCondition[] {
                new ArgumentsCondition(new AttrString("1")),
                new NameCondition("??st")
            });
            _scriptProcessor.RegisterHandler(cond, (name, args) => {
                evName = name;
            });

            _processor.Process(new OtherTestEvent{ Value = 2 });
            Assert.IsNull(evName);
            _processor.Process(new OtherTestEvent{ Value = 1 });
            Assert.AreEqual("other", evName);
            _processor.Process(new TestEvent{ Value = "1" });
            Assert.AreEqual("test", evName);
        }

    }

}