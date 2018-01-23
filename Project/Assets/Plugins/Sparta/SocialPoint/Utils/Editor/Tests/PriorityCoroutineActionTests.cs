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
        int _run = 0;

        IEnumerator Wait(int amount, int run)
        {
            while (amount-- > 0)
            {
                yield return null;   
            }
            _run = run;
        }

        [Test]
        public void Complex()
        {
            var queue = new PriorityCoroutineAction();
            queue.Add(-10, Wait(0, 7));
            queue.Add(-10, Wait(1, 8));
            queue.Add(0, Wait(0, 4));
            queue.Add(-10, Wait(2, 9));
            queue.Add(0, Wait(1, 5));
            queue.Add(0, Wait(2, 6));
            queue.Add(10, Wait(1, 2));
            queue.Add(10, Wait(0, 1));
            queue.Add(10, Wait(2, 3));

            var itr = queue.RunCoroutine();
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

