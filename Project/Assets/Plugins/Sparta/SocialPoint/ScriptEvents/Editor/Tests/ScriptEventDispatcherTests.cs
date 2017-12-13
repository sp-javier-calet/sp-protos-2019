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
        public void Script_Raise_Calls_Listener()
        {
            string val = null;
            _dispatcher.AddListener<TestEvent>(ev => {
                val = ev.Value;
            });
            _scriptDispatcher.Raise("test", _testArgs);
            Assert.AreEqual(_testEvent.Value, val);
        }

        [Test]
        public void Script_Raise_Calls_Script_Listener()
        {
            Attr val = null;
            _scriptDispatcher.AddListener("test", args => {
                val = (Attr)args.Clone();
            });
            _scriptDispatcher.Raise("test", _testArgs);
            Assert.AreEqual(_testArgs, val);
        }

        [Test]
        public void Raise_Calls_Script_Listener()
        {
            Attr val = null;
            _scriptDispatcher.AddListener("test", args => {
                val = (Attr)args.Clone();
            });
            _dispatcher.Raise(_testEvent);
            Assert.AreEqual(_testEvent.Value, val.ToString());
        }

        [Test]
        public void Bridge_Load_Called()
        {
            var bridge = Substitute.For<IScriptEventsBridge>();
			
            _scriptDispatcher.AddBridge(bridge);
            _scriptDispatcher.AddBridge(bridge);
			
            bridge.Received(1).Load(_scriptDispatcher);
        }

        [Test]
        public void Bridge_Dispose_Called()
        {
            var bridge = Substitute.For<IScriptEventsBridge>();
			
            _scriptDispatcher.AddBridge(bridge);
            _scriptDispatcher.Dispose();
			
            bridge.Received().Dispose();
        }

        [Test]
        public void NameCondition_Works()
        {
            string evName = null;
            var cond = new NameCondition("te*");
            _scriptDispatcher.AddListener(cond, (name, args) => {
                evName = name;
            });

            _dispatcher.Raise(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _dispatcher.Raise(new TestEvent{ Value = "lala" });
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void ArgumentCondition_Works()
        {
            string evName = null;
            var cond = new ArgumentsCondition(_testArgs);
            _scriptDispatcher.AddListener(cond, (name, args) => {
                evName = name;
            });
			
            _dispatcher.Raise(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _dispatcher.Raise(new TestEvent{ Value = "lala" });
            Assert.IsNull(evName);
            _dispatcher.Raise(_testEvent);
            Assert.AreEqual("test", evName);
        }

        [Test]
        public void NotCondition_Works()
        {
            string evName = null;
            var cond = new NotCondition(new ArgumentsCondition(_testArgs));
            _scriptDispatcher.AddListener(cond, (name, args) => {
                evName = name;
            });
			
            _dispatcher.Raise(_testEvent);
            Assert.IsNull(evName);
            _dispatcher.Raise(new TestEvent{ Value = "lala" });
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
            _scriptDispatcher.AddListener(cond, (name, args) => {
                evName = name;
            });
			
            _dispatcher.Raise(new OtherTestEvent{ Value = 1 });
            Assert.IsNull(evName);
            _dispatcher.Raise(_testEvent);
            Assert.IsNull(evName);
            _dispatcher.Raise(new TestEvent{ Value = "1" });
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
            _scriptDispatcher.AddListener(cond, (name, args) => {
                evName = name;
            });

            _dispatcher.Raise(new OtherTestEvent{ Value = 2 });
            Assert.IsNull(evName);
            _dispatcher.Raise(new OtherTestEvent{ Value = 1 });
            Assert.AreEqual("other", evName);
            _dispatcher.Raise(new TestEvent{ Value = "1" });
            Assert.AreEqual("test", evName);
        }

    }

}