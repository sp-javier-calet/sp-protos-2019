using NUnit.Framework;
using NSubstitute;
using System;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
	[TestFixture]
	[Category("SocialPoint.ScriptEvents")]
	internal class EventScriptTests : BaseScriptEventsTests
	{
		EventScript _eventScript;

		[SetUp]
		override public void SetUp()
		{
			base.SetUp();
		}

		[Test]
		public void Empty_Finishes()
		{
			var script = new EventScript(_scriptDispatcher, new EventScriptStepModel[]{});
			bool finished = false;
			script.Run(() => {
				finished = true;
			});
			Assert.IsTrue(finished);
		}

		[Test]
		public void Simple_Finishes()
		{
			var script = new EventScript(_scriptDispatcher, new EventScriptStepModel[]{
				new EventScriptStepModel{
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
			var script = new EventScript(_scriptDispatcher, new EventScriptStepModel[]{
				new EventScriptStepModel{
					EventName = "test",
					EventArguments = new AttrString("lala"),
					Condition = new NameCondition("other")
				},
				new EventScriptStepModel{
					EventName = "other",
					EventArguments = new AttrInt(1),
					Condition = new ArgumentsCondition(_testArgs)
				},
			});
			
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