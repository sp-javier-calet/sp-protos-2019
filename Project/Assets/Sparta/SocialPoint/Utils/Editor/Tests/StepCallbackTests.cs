using System;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    class StepCallbackTests
    {
        bool _allStepsDone;

        [SetUp]
        public void SetUp()
        {
            _allStepsDone = false;
        }

        void OnAllStepsDone()
        {
            _allStepsDone = true;
        }

        [Test]
        public void ReadyCalled()
        {   
            Action callback = OnAllStepsDone;

            var steps = new StepCallbackBuilder(callback);

            SomeMethod(steps.Add());
            SomeMethod(steps.Add());

            steps.Ready();

            Assert.IsTrue(_allStepsDone);
        }

        [Test]
        public void ReadyNotCalled()
        {   
            Action callback = OnAllStepsDone;

            var steps = new StepCallbackBuilder(callback);

            SomeMethod(steps.Add());
            SomeMethod(steps.Add());

            Assert.IsTrue(!_allStepsDone);
        }

        [Test]
        public void AddAfterReady()
        {   
            Action callback = OnAllStepsDone;

            var steps = new StepCallbackBuilder(callback);

            SomeMethod(steps.Add());

            steps.Ready();

            if(_allStepsDone)
            {
                return;
            }

            SomeMethod(steps.Add());
        }

        static void SomeMethod(Action callback)
        {
            // do stuff

            if(callback != null)
            {
                callback();
            }
        }
    }
}
