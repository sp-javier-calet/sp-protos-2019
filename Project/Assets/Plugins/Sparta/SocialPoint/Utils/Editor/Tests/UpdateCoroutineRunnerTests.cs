using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class UpdateCoroutineRunnerTests
    {
        UpdateScheduler _scheduler;
        ICoroutineRunner _coroutineRunner;

        [SetUp]
        public void SetUp()
        {
            _scheduler = new UpdateScheduler();
            _coroutineRunner = new UpdateCoroutineRunner(_scheduler);
        }

        protected void UpdateScheduler()
        {
            _scheduler.Update(0.0f, 0.0f);
        }

        [Test]
        public void StartCoroutine()
        {
            var enumerator = Substitute.For<IEnumerator>();
            enumerator.MoveNext().Returns(true);
            _coroutineRunner.StartCoroutine(enumerator);
            UpdateScheduler();

            enumerator.Received(1).MoveNext();
        }

        [Test]
        public void ExecuteTwiceCoroutine()
        {
            var enumerator = Substitute.For<IEnumerator>();
            enumerator.MoveNext().Returns(true);
            _coroutineRunner.StartCoroutine(enumerator);
            UpdateScheduler();
            UpdateScheduler();

            enumerator.Received(2).MoveNext();
        }

        [Test]
        public void StartAndStopCoroutine()
        {
            var enumerator = Substitute.For<IEnumerator>();
            enumerator.MoveNext().Returns(true);
            var handler = _coroutineRunner.StartCoroutine(enumerator);
            UpdateScheduler();
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();

            enumerator.Received(1).MoveNext();
        }

        [Test]
        public void StartAndStopCoroutineSameFrame()
        {
            var enumerator = Substitute.For<IEnumerator>();
            enumerator.MoveNext().Returns(true);
            var handler = _coroutineRunner.StartCoroutine(enumerator);
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();
            UpdateScheduler();

            enumerator.Received(0).MoveNext();
        }

        [Test]
        public void StopCoroutineDuringUpdate()
        {
            var coroutine1 = Substitute.For<IEnumerator>();
            coroutine1.MoveNext().Returns(true);
            var handler = _coroutineRunner.StartCoroutine(coroutine1);

            var coroutine2 = Substitute.For<IEnumerator>();
            coroutine2.MoveNext().Returns(true);
            coroutine2.When(x => x.MoveNext())
                .Do(x => _coroutineRunner.StopCoroutine(handler));

            _coroutineRunner.StartCoroutine(coroutine2);

            UpdateScheduler();
            UpdateScheduler();

            coroutine1.Received(1).MoveNext();
            coroutine2.Received(2).MoveNext();
        }

        [Test]
        public void StartTwoCoroutines()
        {
            var coroutine1 = Substitute.For<IEnumerator>();
            coroutine1.MoveNext().Returns(true);
            var coroutine2 = Substitute.For<IEnumerator>();
            coroutine2.MoveNext().Returns(true);
            _coroutineRunner.StartCoroutine(coroutine1);
            _coroutineRunner.StartCoroutine(coroutine2);
            UpdateScheduler();
            UpdateScheduler();

            coroutine1.Received(2).MoveNext();
            coroutine2.Received(2).MoveNext();
        }

        [Test]
        public void StartTwoCoroutinesAndStopOne()
        {
            var coroutine1 = Substitute.For<IEnumerator>();
            coroutine1.MoveNext().Returns(true);
            var coroutine2 = Substitute.For<IEnumerator>();
            coroutine2.MoveNext().Returns(true);
            var handler = _coroutineRunner.StartCoroutine(coroutine1);
            _coroutineRunner.StartCoroutine(coroutine2);
            UpdateScheduler();
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();

            coroutine1.Received(1).MoveNext();
            coroutine2.Received(2).MoveNext();
        }

        class StepsCoroutine
        {
            public int MaxSteps{ get; private set; }

            public int CurrentSteps { get; private set; }

            public bool HasFinished
            { 
                get
                {
                    return CurrentSteps == MaxSteps;
                }
            }

            public StepsCoroutine(int steps)
            {
                MaxSteps = steps;
                CurrentSteps = 0;
            }

            public IEnumerator Coroutine()
            {
                while(CurrentSteps < MaxSteps)
                {
                    CurrentSteps++;
                    yield return null;
                }
            }
        }

        [Test]
        public void CoroutineWithSteps()
        {
            var coroutine = new StepsCoroutine(5);
            _coroutineRunner.StartCoroutine(coroutine.Coroutine());

            for(int i = 0; i < coroutine.MaxSteps; i++)
            {
                Assert.AreEqual(i, coroutine.CurrentSteps);
                UpdateScheduler();
            }

            Assert.AreEqual(coroutine.MaxSteps, coroutine.CurrentSteps);
            Assert.IsTrue(coroutine.HasFinished);

            UpdateScheduler();

            Assert.AreEqual(coroutine.MaxSteps, coroutine.CurrentSteps);
            Assert.IsTrue(coroutine.HasFinished);
        }

        class NestedStepsCoroutine
        {
            public int MaxSteps
            {
                get
                {
                    //Plus two because:
                    //* first yield return InnerCoroutine.Coroutine() yields without executing 
                    //* when it finishes it also yields one more time
                    return InnerCoroutine.MaxSteps + 2;
                }
            }

            public bool HasFinished{ get; private set; }

            public StepsCoroutine InnerCoroutine{ get; private set; }

            public NestedStepsCoroutine(int steps)
            {
                HasFinished = false;
                InnerCoroutine = new StepsCoroutine(steps);
            }

            public IEnumerator Coroutine()
            {
                yield return InnerCoroutine.Coroutine();
                HasFinished = true;
            }
        }

        [Test]
        public void NestedCoroutineWithSteps()
        {
            var coroutine = new NestedStepsCoroutine(10);
            _coroutineRunner.StartCoroutine(coroutine.Coroutine());

            for(int i = 0; i < coroutine.MaxSteps; i++)
            {
                UpdateScheduler();
            }

            Assert.AreEqual(coroutine.InnerCoroutine.MaxSteps, coroutine.InnerCoroutine.CurrentSteps);
            Assert.IsTrue(coroutine.InnerCoroutine.HasFinished);
            Assert.IsTrue(coroutine.HasFinished);

            UpdateScheduler();

            Assert.AreEqual(coroutine.InnerCoroutine.MaxSteps, coroutine.InnerCoroutine.CurrentSteps);
            Assert.IsTrue(coroutine.InnerCoroutine.HasFinished);
            Assert.IsTrue(coroutine.HasFinished);
        }

        IEnumerator InvalidYield()
        {
            yield return new Object();
        }

        [Test]
        public void InvalidYieldCoroutine()
        {
            _coroutineRunner.StartCoroutine(InvalidYield());

            Assert.Throws(Is.InstanceOf<System.Exception>(), () => {
                for(int i = 0; i < 50; i++)
                {
                    UpdateScheduler();
                }
            });
        }

        IEnumerator UnityYield()
        {
            yield return new WaitForSeconds(1.0f);
        }

        [Test]
        public void UnityYieldCoroutine()
        {
            _coroutineRunner.StartCoroutine(UnityYield());

            Assert.Throws(Is.InstanceOf<System.Exception>(), () => {
                for(int i = 0; i < 50; i++)
                {
                    UpdateScheduler();
                }
            });
        }
    }
}
