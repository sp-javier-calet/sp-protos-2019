using NUnit.Framework;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    [TestFixture]
    [Category("SocialPoint.ScriptEvents")]
    class ScriptTests : BaseScriptEventsTests
    {
        [SetUp]
        public void SetUpBase()
        {
            SetUp();
        }

        [Test]
        public void Empty_Finishes()
        {
            var script = new Script(_scriptProcessor, new ScriptStepModel[]{ });
            bool finished = false;
            script.Run(() => {
                finished = true;
            });
            Assert.IsTrue(finished);
        }

        [Test]
        public void Simple_Finishes()
        {
            var script = new Script(_scriptProcessor, new [] {
                new ScriptStepModel {
                    Name = "test",
                    Arguments = new AttrString("lala")
                }
            });

            string arg = null;
            _processor.RegisterHandler<TestEvent>(ev => {
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
            var script = new Script(_scriptProcessor, new [] {
                new ScriptStepModel {
                    Name = "test",
                    Arguments = new AttrString("lala"),
                    Forward = new NameCondition("other")
                },
                new ScriptStepModel {
                    Name = "other",
                    Arguments = new AttrString("1"),
                    Forward = new ArgumentsCondition(_testArgs),
                    Backward = new FixedCondition(true)
                },
            });
							
            TestScript(script);
        }

        [Test]
        public void Load_From_Attr()
        {
            const string json = @"
[{
	""name"": ""test"",
	""args"": ""lala"",
	""forward"": {
		""type"": ""name"",
		""value"": ""other""
	}
},{
	""name"": ""other"",
	""args"": ""1"",
	""forward"": {
		""type"": ""args"",
		""value"": ""test_value""
	},
	""backward"": {
        ""type"": ""all""
    }
}]
";
            var data = new JsonAttrParser().ParseString(json);
            var scriptModel = new ScriptModelParser().Parse(data);
            var script = new Script(_scriptProcessor, scriptModel);
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
			
            _processor.Process(new OtherTestEvent{ Value = 1 });
			
            Assert.AreEqual(1, script.CurrentStepNum);
            Assert.IsTrue(script.IsRunning);
			
            _processor.Process(new TestEvent{ Value = "lala" });
			
            Assert.AreEqual(0, script.CurrentStepNum);
            Assert.IsTrue(script.IsRunning);
			
            _processor.Process(new OtherTestEvent{ Value = 1 });
			
            Assert.AreEqual(1, script.CurrentStepNum);
            Assert.IsTrue(script.IsRunning);
			
            _processor.Process(_testEvent);
			
            Assert.AreEqual(2, script.CurrentStepNum);
            Assert.IsFalse(script.IsRunning);
            Assert.IsTrue(script.IsFinished);
            Assert.IsTrue(finished);
        }


    }

}