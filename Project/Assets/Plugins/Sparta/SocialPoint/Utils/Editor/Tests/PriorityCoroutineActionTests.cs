using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public sealed class PriorityCoroutineActionTests
    {
        int _run;

        IEnumerator Wait(int amount, int run)
        {
            while (amount-- > 0)
            {
                yield return null;   
            }
            _run = run;
        }

        [SetUp]
        public void SetUp()
        {
            _run = 0;
        }

        [Test]
        public void RunTwoTimes()
        {
            var action = new PriorityCoroutineAction();
            action.Add(0, () => Wait(0, 1));

            var itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(1, _run);
            _run = 0;
            itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(1, _run);
        }

        [Test]
        public void Remove()
        {
            var action = new PriorityCoroutineAction();
            Func<IEnumerator> clk1 = () => { return Wait(0, 1); };
            Func<IEnumerator> clk2 = () => { return Wait(0, 2); };
            action.Add(10, clk1);
            action.Add(0, clk2);

            var itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(1, _run);
            itr.MoveNext();
            Assert.AreEqual(2, _run);

            _run = 0;
            action.Remove(clk1);
            itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(2, _run);

            _run = 0;
            action.Remove(clk2);
            itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(0, _run);
            itr.MoveNext();
            Assert.AreEqual(0, _run);
        }

        [Test]
        public void DefaultListener()
        {
            var action = new PriorityCoroutineAction();
            action.Add((int prio) =>
            {
                return Wait(0, 4+prio);
            });

            action.Add(10, () => Wait(1, 1));
            action.Add(0, () => Wait(1, 2));
            action.Add(-10, () => Wait(0, 3));

            var itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(14, _run);
            itr.MoveNext();
            Assert.AreEqual(1, _run);
            itr.MoveNext();
            Assert.AreEqual(4, _run);
            itr.MoveNext();
            Assert.AreEqual(2, _run);
            itr.MoveNext();
            Assert.AreEqual(3, _run);
        }

        [Test]
        public void Complex()
        {
            var action = new PriorityCoroutineAction();
            action.Add(-10, () => Wait(0, 7));
            action.Add(-10, () => Wait(1, 8));
            action.Add(0, () => Wait(0, 4));
            action.Add(-10, () => Wait(2, 9));
            action.Add(0, () => Wait(1, 5));
            action.Add(0, () => Wait(2, 6));
            action.Add(10, () => Wait(1, 2));
            action.Add(10, () => Wait(0, 1));
            action.Add(10, () => Wait(2, 3));

            var itr = action.RunCoroutine();
            itr.MoveNext();
            Assert.AreEqual(1, _run);
            itr.MoveNext();
            Assert.AreEqual(2, _run);
            itr.MoveNext();
            Assert.AreEqual(3, _run);
            itr.MoveNext();
            Assert.AreEqual(4, _run);
            itr.MoveNext();
            Assert.AreEqual(5, _run);
            itr.MoveNext();
            Assert.AreEqual(6, _run);
            itr.MoveNext();
            Assert.AreEqual(7, _run);
            itr.MoveNext();
            Assert.AreEqual(8, _run);
            itr.MoveNext();
            Assert.AreEqual(9, _run);
        }
    }
}

