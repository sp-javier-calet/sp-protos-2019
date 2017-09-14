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

        class ExecutionCounter : IEnumerator
        {
            public int TimesExecuted{ get; private set; }

            public bool MoveNext()
            {
                TimesExecuted++;
                return true;
            }

            public void Reset()
            {
            }

            public object Current
            {
                get
                {
                    return null;
                }
            }
        }

        [Test]
        public void StartCoroutine()
        {
            var coroutine = new ExecutionCounter();
            _coroutineRunner.StartCoroutine(coroutine);
            UpdateScheduler();

            Assert.AreEqual(1, coroutine.TimesExecuted);
        }

        [Test]
        public void ExecuteTwiceCoroutine()
        {
            var coroutine = new ExecutionCounter();
            _coroutineRunner.StartCoroutine(coroutine);
            UpdateScheduler();
            UpdateScheduler();

            Assert.AreEqual(2, coroutine.TimesExecuted);
        }

        [Test]
        public void StartAndStopCoroutine()
        {
            var coroutine = new ExecutionCounter();
            var handler = _coroutineRunner.StartCoroutine(coroutine);
            UpdateScheduler();
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();

            Assert.AreEqual(1, coroutine.TimesExecuted);
        }

        [Test]
        public void StartAndStopCoroutineSameFrame()
        {
            var coroutine = new ExecutionCounter();
            var handler = _coroutineRunner.StartCoroutine(coroutine);
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();
            UpdateScheduler();

            Assert.AreEqual(0, coroutine.TimesExecuted);
        }

        [Test]
        public void StopCoroutineDuringUpdate()
        {
            var coroutine1 = new ExecutionCounter();
            var handler = _coroutineRunner.StartCoroutine(coroutine1);

            int timesExecuted = 0;
            var coroutine2 = Substitute.For<IEnumerator>();
            coroutine2.MoveNext().Returns(true);
            coroutine2.When(x => x.MoveNext())
                .Do(x => {
                timesExecuted++;
                _coroutineRunner.StopCoroutine(handler);
            });

            _coroutineRunner.StartCoroutine(coroutine2);

            UpdateScheduler();
            UpdateScheduler();

            Assert.AreEqual(1, coroutine1.TimesExecuted);
            Assert.AreEqual(2, timesExecuted);
        }

        [Test]
        public void StartTwoCoroutines()
        {
            var coroutine1 = new ExecutionCounter();
            var coroutine2 = new ExecutionCounter();
            _coroutineRunner.StartCoroutine(coroutine1);
            _coroutineRunner.StartCoroutine(coroutine2);
            UpdateScheduler();
            UpdateScheduler();

            Assert.AreEqual(2, coroutine1.TimesExecuted);
            Assert.AreEqual(2, coroutine2.TimesExecuted);
        }

        [Test]
        public void StartTwoCoroutinesAndStopOne()
        {
            var coroutine1 = new ExecutionCounter();
            var coroutine2 = new ExecutionCounter();
            var handler = _coroutineRunner.StartCoroutine(coroutine1);
            _coroutineRunner.StartCoroutine(coroutine2);
            UpdateScheduler();
            _coroutineRunner.StopCoroutine(handler);
            UpdateScheduler();

            Assert.AreEqual(1, coroutine1.TimesExecuted);
            Assert.AreEqual(2, coroutine2.TimesExecuted);
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
