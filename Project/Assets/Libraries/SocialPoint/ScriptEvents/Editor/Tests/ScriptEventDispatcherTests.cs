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


	}

}