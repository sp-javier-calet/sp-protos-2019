using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
	internal struct ScriptTestEvent
	{
		public string Value;
	}

	internal struct OtherScriptTestEvent
	{
		public int Value;
	}

	class ScriptTestEventConverter : BaseScriptEventConverter<ScriptTestEvent>
	{
		public ScriptTestEventConverter(): base("test")
		{
		}
		
		override protected ScriptTestEvent ParseEvent(Attr data)
		{
			return new ScriptTestEvent{
				Value = data.AsValue.ToString()
			};
		}
		
		override protected Attr SerializeEvent(ScriptTestEvent ev)
		{
			return new AttrString(ev.Value);
		}
	}

	class OtherScriptTestEventConverter : BaseScriptEventConverter<OtherScriptTestEvent>
	{
		public OtherScriptTestEventConverter(): base("other")
		{
		}
		
		override protected OtherScriptTestEvent ParseEvent(Attr data)
		{
			return new OtherScriptTestEvent{
				Value = data.AsValue.ToInt()
			};
		}
		
		override protected Attr SerializeEvent(OtherScriptTestEvent ev)
		{
			return new AttrInt(ev.Value);
		}
	}

	[TestFixture]
	[Category("SocialPoint.ScriptEvents")]
	internal class ScriptEventDispatcherTests
	{

		EventDispatcher _dispatcher;
		ScriptEventDispatcher _scriptDispatcher;
		ScriptTestEvent _testEvent;
		Attr _testArgs;

		[SetUp]
		public void SetUp()
		{
			_dispatcher = new EventDispatcher();
			_scriptDispatcher = new ScriptEventDispatcher(_dispatcher);
			var testConv = new ScriptTestEventConverter();
			_scriptDispatcher.AddConverter(testConv);
			_testEvent = new ScriptTestEvent{ Value = "test_value" };
			_testArgs = testConv.Serialize(_testEvent);
		}

		[Test]
		public void Script_Raise_Calls_Listener()
		{
			string val = null;
			_dispatcher.AddListener<ScriptTestEvent>((ev) => {
				val = ev.Value;
			});
			_scriptDispatcher.Raise("test", _testArgs);			
			Assert.AreEqual(_testEvent.Value, val);
		}

		[Test]
		public void Script_Raise_Calls_Script_Listener()
		{
			Attr val = null;
			_scriptDispatcher.AddListener("test", (args) => {
				val = (Attr) args.Clone();
			});
			_scriptDispatcher.Raise("test", _testArgs);			
			Assert.AreEqual(_testArgs, val);
		}

		[Test]
		public void Raise_Calls_Script_Listener()
		{
			Attr val = null;
			_scriptDispatcher.AddListener("test", (args) => {
				val = (Attr) args.Clone();
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
			_scriptDispatcher.AddListener(new NameCondition("te*"), (name, args) => {
				evName = name;
			});

			_dispatcher.Raise(new OtherScriptTestEvent{ Value = 1 });
			Assert.IsNull(evName);
			_dispatcher.Raise(new ScriptTestEvent{ Value = "lala" });
			Assert.AreEqual("test", evName);
		}

		[Test]
		public void ArgumentCondition_Works()
		{
			string evName = null;
			_scriptDispatcher.AddListener(new ArgumentsCondition(_testArgs), (name, args) => {
				evName = name;
			});
			
			_dispatcher.Raise(new OtherScriptTestEvent{ Value = 1 });
			Assert.IsNull(evName);
			_dispatcher.Raise(new ScriptTestEvent{ Value = "lala" });
			Assert.IsNull(evName);
			_dispatcher.Raise(_testEvent);
			Assert.AreEqual("test", evName);
		}

		[Test]
		public void NotCondition_Works()
		{
			string evName = null;
			_scriptDispatcher.AddListener(new NotCondition(new ArgumentsCondition(_testArgs)), (name, args) => {
				evName = name;
			});
			
			_dispatcher.Raise(_testEvent);
			Assert.IsNull(evName);
			_dispatcher.Raise(new ScriptTestEvent{ Value = "lala" });
			Assert.AreEqual("test", evName);
		}

	}

}