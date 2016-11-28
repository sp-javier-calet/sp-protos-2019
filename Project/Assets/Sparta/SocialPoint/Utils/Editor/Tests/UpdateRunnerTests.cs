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
    }
}
