using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
	[TestFixture]
	[Category("SocialPoint.ScriptEvents")]
	internal class ScriptTests : BaseScriptEventsTests
	{
		Script _eventScript;

		[SetUp]
		override public void SetUp()
		{
			base.SetUp();
		}

		[Test]
		public void Empty_Finishes()
		{
			var script = new Script(_scriptDispatcher, new ScriptStepModel[]{});
			bool finished = false;
			script.Run(() => {
				finished = true;
			});
			Assert.IsTrue(finished);
		}

		[Test]
		public void Simple_Finishes()
		{
			var script = new Script(_scriptDispatcher, new ScriptStepModel[]{
				new ScriptStepModel{
					EventName = "test",
					EventArguments = new AttrString("lala")
				}
			});

			string arg = null;
			_dispatcher.AddListener<TestEvent>((ev) => {
				arg = ev.Value;
			});

			bool finished = false;
			script.Run(() => {
				finished = true;
			});
			Assert.IsTrue(finished);
			Assert.AreEqual("lala", arg);
		}

		[Test]
		public void Multi_Finishes()
		{
			var script = new Script(_scriptDispatcher, new ScriptStepModel[]{
				new ScriptStepModel{
					EventName = "test",
					EventArguments = new AttrString("lala"),
					Forward = new NameCondition("other")
				},
				new ScriptStepModel{
					EventName = "other",
					EventArguments = new AttrString("1"),
					Forward = new ArgumentsCondition(_testArgs)
				},
			});
			
			TestScript(script);
		}

		[Test]
		public void Load_From_Attr()
		{
			var json = @"
[{
	""event_name"": ""test"",
	""event_args"": ""lala"",
	""forward"": {
		""type"": ""name"",
		""value"": ""other""
	}
},{
	""event_name2"": ""other"",
	""event_args"": ""1"",
	""forward"": {
		""type"": ""args"",
		""value"": ""test_value""
	}
}]
";
			var data = new JsonAttrParser().ParseString(json);
			var script = new ScriptParser(_scriptDispatcher).Parse(data);
			TestScript(script);
		}

		void TestScript(Script script)
		{
			bool finished = false;
			script.Run(() => {
				finished = true;
			});
			Assert.AreEqual(0, script.CurrentStepNum);
			Assert.IsTrue(script.IsRunning);
			
			_dispatcher.Raise(new OtherTestEvent{ Value = 1 });
			
			Assert.AreEqual(1, script.CurrentStepNum);
			Assert.IsTrue(script.IsRunning);
			
			_dispatcher.Raise(new TestEvent{ Value = "lala" });
			
			Assert.AreEqual(0, script.CurrentStepNum);
			Assert.IsTrue(script.IsRunning);
			
			_dispatcher.Raise(new OtherTestEvent{ Value = 1 });
			
			Assert.AreEqual(1, script.CurrentStepNum);
			Assert.IsTrue(script.IsRunning);
			
			_dispatcher.Raise(_testEvent);
			
			Assert.AreEqual(2, script.CurrentStepNum);
			Assert.IsFalse(script.IsRunning);
			Assert.IsTrue(script.IsFinished);
			Assert.IsTrue(finished);
		}


	}

}