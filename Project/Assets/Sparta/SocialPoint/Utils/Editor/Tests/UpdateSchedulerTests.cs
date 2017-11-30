using System;
using NSubstitute;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class UpdateSchedulerTests
    {
        UpdateScheduler _scheduler;

        class UpdateAction : IUpdateable
        {
            readonly Action _update;

            public UpdateAction(Action cbk)
            {
                _update = cbk;
            }

            public void Update()
            {
                _update();
            }
        }

        [SetUp]
        public void SetUp()
        {
            _scheduler = new UpdateScheduler();
        }

        [Test]
        public void RemoveTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);

            _scheduler.Remove(updateable);
            _scheduler.Remove(updateable);

            Assert.IsFalse(_scheduler.Contains(updateable));
            _scheduler.Update(0.1f, 0.1f);
            Assert.IsFalse(_scheduler.Contains(updateable));
        }

        [Test]
        public void UpdateTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f, 0.1f);
            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(2).Update();
        }

        [Test]
        public void RemoveBeforeAdd()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            _scheduler.Remove(updateable);
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();
        }

        [Test]
        public void RemoveBeforeAddFixed()
        {
            var updateable = Substitute.For<IUpdateable>();

            // Testing deprecated AddFixed method
            #pragma warning disable 618
            _scheduler.AddFixed(updateable, 0);
            _scheduler.Remove(updateable);
            _scheduler.AddFixed(updateable, 0);
            #pragma warning restore 618

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();
        }

        [Test]
        public void RemoveAfterUpdate()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            Assert.IsTrue(_scheduler.Contains(updateable));
            _scheduler.Update(0.1f, 0.1f);
            DoRemove(updateable);
        }

        [Test]
        public void Remove()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            DoRemove(updateable);
        }

        public void DoRemove(IUpdateable updateable)
        {
            Assert.IsTrue(_scheduler.Contains(updateable));
            _scheduler.Remove(updateable);
            Assert.IsFalse(_scheduler.Contains(updateable));
        }

        public IUpdateable DoAddTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();

            return updateable;
        }

        public IUpdateable DoAddFixedTwice()
        {
            var updateable = Substitute.For<IUpdateable>();

            // Tests deprecated method AddFixed
            #pragma warning disable 618
            _scheduler.AddFixed(updateable, 0);
            _scheduler.AddFixed(updateable, 0);
            #pragma warning restore 618

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();

            return updateable;
        }

        [Test]
        public void AddTwice()
        {
            DoAddTwice();
        }

        [Test]
        public void AddFixedTwice()
        {
            DoAddFixedTwice();
        }

        [Test]
        public void AddTwiceAndRemove()
        {
            var updateable = DoAddTwice();
            DoRemove(updateable);
        }

        [Test]
        public void AddFixedTwiceAndRemove()
        {
            var updateable = DoAddFixedTwice();
            DoRemove(updateable);
        }

        [Test]
        public void Add()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);

            Assert.IsTrue(_scheduler.Contains(updateable));
            _scheduler.Update(0.1f, 0.1f);
            Assert.IsTrue(_scheduler.Contains(updateable));
        }

        [Test]
        public void AddInUpdate()
        {
            var updateable = Substitute.For<IUpdateable>();
            var updateableAction = new UpdateAction(() => _scheduler.Add(updateable));
            _scheduler.Add(updateableAction);
            Assert.IsTrue(_scheduler.Contains(updateableAction));

            _scheduler.Update(0.1f, 0.1f);
            Assert.IsTrue(_scheduler.Contains(updateable));
        }

        [Test]
        public void RemoveInUpdate()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            Assert.IsTrue(_scheduler.Contains(updateable));

            var updateableAction = new UpdateAction(() => _scheduler.Remove(updateable));
            _scheduler.Add(updateableAction);
            Assert.IsTrue(_scheduler.Contains(updateableAction));

            _scheduler.Update(0.1f, 0.1f);
            Assert.IsFalse(_scheduler.Contains(updateable));
        }

        public IUpdateable DoAddAndAddFixed()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            #pragma warning disable 618
            _scheduler.AddFixed(updateable, 0);
            #pragma warning restore 618

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();
            return updateable;
        }

        public IUpdateable DoAddFixedAndAdd()
        {
            var updateable = Substitute.For<IUpdateable>();
            #pragma warning disable 618
            _scheduler.AddFixed(updateable, 0);
            #pragma warning restore 618
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f, 0.1f);

            updateable.Received(1).Update();
            return updateable;
        }

        [Test]
        public void AddAndAddFixed()
        {
            DoAddAndAddFixed();
        }

        [Test]
        public void AddFixedAndAdd()
        {
            DoAddFixedAndAdd();
        }

        [Test]
        public void AddAndAddFixedAndRemove()
        {
            var updateable = DoAddAndAddFixed();
            DoRemove(updateable);
        }

        [Test]
        public void AddFixedAndAddAndRemove()
        {
            var updateable = DoAddFixedAndAdd();
            DoRemove(updateable);
        }

        [Test]
        public void ScaledGameSlower()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.GameTimeScaled, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(200);
            _scheduler.Update(1f, 1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void UnscaledGameSlower()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.GameTimeUnscaled, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(200);
            _scheduler.Update(1f, 1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void ScaledGameFaster()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.GameTimeScaled, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(50);
            _scheduler.Update(0.1f, 0.1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void UnscaledGameFaster()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.GameTimeUnscaled, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(50);
            _scheduler.Update(0.1f, 0.1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void RealTimeSlower()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.RealTime, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.05f, 0.05f);
            updateable.Received(1).Update();
        }

        [Test]
        public void RealTimeFaster()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, UpdateableTimeMode.RealTime, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.2f, 0.2f);
            updateable.Received(1).Update();
        }

        [Test]
        public void GameVsRealTime()
        {
            var updateable0 = Substitute.For<IUpdateable>();
            var updateable1 = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable0, UpdateableTimeMode.RealTime, 0.1f);
            _scheduler.Add(updateable1, UpdateableTimeMode.GameTimeUnscaled, 0.1f);
            _scheduler.Update(0, 0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.05f, 0.05f);
            updateable0.Received(1).Update();
            updateable1.Received(0).Update();
        }

        [Test]
        public void DeleteAndAddWithDifferentConfig()
        {
            var updateable0 = Substitute.For<IUpdateable>();
            var updateable1 = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable0, UpdateableTimeMode.GameTimeScaled, 0.1f);
            _scheduler.Add(updateable1, UpdateableTimeMode.GameTimeScaled, 0.05f);
            _scheduler.Remove(updateable0);
            _scheduler.Remove(updateable1);
            _scheduler.Add(updateable0, UpdateableTimeMode.GameTimeScaled, 0.05f);
            _scheduler.Add(updateable1, UpdateableTimeMode.GameTimeScaled, 0.1f);
            _scheduler.Update(0.05f, 0.05f);
            updateable0.Received(1).Update();
            updateable1.Received(0).Update();
        }

        [Test]
        public void ExceptionDuringUpdate()
        {
            var updateable0 = Substitute.For<IUpdateable>();
            updateable0.When(x => x.Update()).Do(x => {
                throw new Exception();
            });
            var updateable1 = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable0);
            _scheduler.Add(updateable1);

            Assert.Throws<AggregateException>(() => _scheduler.Update(0.05f, 0.05f));

            updateable0.Received(1).Update();
            updateable1.Received(1).Update();
        }

        [Test]
        public void ExceptionDuringUpdateCallback()
        {
            var exception = new Exception();
            var updateable = Substitute.For<IUpdateable>();
            updateable.When(x => x.Update()).Do(x => {
                throw exception;
            });

            bool callbackCalled = false;
            _scheduler.UpdateExceptionThrown += e => {
                callbackCalled = true;
                Assert.AreEqual(exception, e);
            };

            _scheduler.Add(updateable);

            Assert.Throws<AggregateException>(() => _scheduler.Update(0.05f, 0.05f));

            updateable.Received(1).Update();

            Assert.True(callbackCalled);
        }

        public void RealTimeMillis()
        {
            var updateable = Substitute.For<IDeltaUpdateable<int>>();
            _scheduler.Add(updateable, UpdateableTimeIntMode.RealTime);
            _scheduler.Update(0, 0);
            updateable.Received(1).Update(0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.05f, 0.05f);
            updateable.Received(1).Update(Arg.Is<int>( x => x >= 100 ));
            System.Threading.Thread.Sleep(200);
            _scheduler.Update(0.0f, 0.0f);
            updateable.Received(1).Update(Arg.Is<int>( x => x >= 200 ));
        }

        [Test]
        public void ScaledMillis()
        {
            var updateable = Substitute.For<IDeltaUpdateable<int>>();
            _scheduler.Add(updateable, UpdateableTimeIntMode.GameTimeScaled);
            _scheduler.Update(0, 0);
            updateable.Received(1).Update(0);
            _scheduler.Update(0.05f, 0.1f);
            updateable.Received(1).Update(50);
            _scheduler.Update(0.033f, 0.15f);
            updateable.Received(1).Update(33);
        }

        [Test]
        public void UnscaledMillis()
        {
            var updateable = Substitute.For<IDeltaUpdateable<int>>();
            _scheduler.Add(updateable);
            _scheduler.Update(0, 0);
            updateable.Received(1).Update(0);
            _scheduler.Update(0.05f, 0.1f);
            updateable.Received(1).Update(100);
            _scheduler.Update(0.033f, 0.15f);
            updateable.Received(1).Update(150);
        }

        [Test]
        public void MillisRounding()
        {
            var updateable = Substitute.For<IDeltaUpdateable<int>>();
            _scheduler.Add(updateable);
            _scheduler.Update(0, 0.0014f);
            updateable.Received(1).Update(1);
            _scheduler.Update(0, 0.0015f);
            updateable.Received(1).Update(2);

        }
    }
}
