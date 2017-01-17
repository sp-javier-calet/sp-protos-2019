using NSubstitute;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class UnityUpdaterTests
    {
        UpdateScheduler _scheduler;

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

            Assert.IsTrue(_scheduler.Contains(updateable));
            _scheduler.Update(0.1f);
            Assert.IsFalse(_scheduler.Contains(updateable));
        }

        [Test]
        public void UpdateTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f);
            _scheduler.Update(0.1f);

            updateable.Received(2).Update();
        }

        [Test]
        public void RemoveBeforeAdd()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            _scheduler.Remove(updateable);
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f);

            updateable.Received(1).Update();
        }

        [Test]
        public void RemoveBeforeAddFixed()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.AddFixed(updateable, 0);
            _scheduler.Remove(updateable);
            _scheduler.AddFixed(updateable, 0);

            _scheduler.Update(0.1f);

            updateable.Received(1).Update();
        }

        public void DoRemove(IUpdateable updateable)
        {
            _scheduler.Remove(updateable);
            Assert.IsTrue(_scheduler.Contains(updateable));
            _scheduler.Update(0.1f);
            Assert.IsFalse(_scheduler.Contains(updateable));
        }

        [Test]
        public void Remove()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            DoRemove(updateable);
        }

        public IUpdateable DoAddTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f);

            updateable.Received(1).Update();

            return updateable;
        }

        public IUpdateable DoAddFixedTwice()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.AddFixed(updateable, 0);
            _scheduler.AddFixed(updateable, 0);

            _scheduler.Update(0.1f);

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

        public IUpdateable DoAddAndAddFixed()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable);
            _scheduler.AddFixed(updateable, 0);

            _scheduler.Update(0.1f);

            updateable.Received(1).Update();
            return updateable;
        }

        public IUpdateable DoAddFixedAndAdd()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.AddFixed(updateable, 0);
            _scheduler.Add(updateable);

            _scheduler.Update(0.1f);

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
        public void ScaledSlower()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, true, 0.1f);
            _scheduler.Update(0);
            System.Threading.Thread.Sleep(200);
            _scheduler.Update(1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void ScaledFaster()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, true, 0.1f);
            _scheduler.Update(0);
            System.Threading.Thread.Sleep(50);
            _scheduler.Update(0.1f);
            updateable.Received(1).Update();
        }

        [Test]
        public void NonScaledSlower()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, false, 0.1f);
            _scheduler.Update(0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.05f);
            updateable.Received(1).Update();
        }

        [Test]
        public void NonScaledFaster()
        {
            var updateable = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable, false, 0.1f);
            _scheduler.Update(0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.2f);
            updateable.Received(1).Update();
        }

        [Test]
        public void ScaledAndNonScaled()
        {
            var updateable0 = Substitute.For<IUpdateable>();
            var updateable1 = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable0, false, 0.1f);
            _scheduler.Add(updateable1, true, 0.1f);
            _scheduler.Update(0);
            System.Threading.Thread.Sleep(100);
            _scheduler.Update(0.05f);
            updateable0.Received(1).Update();
            updateable1.Received(0).Update();
        }

        [Test]
        public void DeleteAndAddWithDifferentConfig()
        {
            var updateable0 = Substitute.For<IUpdateable>();
            var updateable1 = Substitute.For<IUpdateable>();
            _scheduler.Add(updateable0, true, 0.1f);
            _scheduler.Add(updateable1, true, 0.05f);
            _scheduler.Remove(updateable0);
            _scheduler.Remove(updateable1);
            _scheduler.Add(updateable0, true, 0.05f);
            _scheduler.Add(updateable1, true, 0.1f);
            _scheduler.Update(0.05f);
            updateable0.Received(1).Update();
            updateable1.Received(0).Update();
        }
    }
}
