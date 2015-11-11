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


	}

}