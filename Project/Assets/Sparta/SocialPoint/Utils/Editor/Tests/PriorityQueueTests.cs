
using NUnit.Framework;
using System;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    internal class PriorityQueueTests
    {

        PriorityQueue<int, string> _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new PriorityQueue<int, string>();
            _queue.Add(-100, "last");
            _queue.Add(100, "important");
            _queue.Add(0, "normal");
        }

        [Test]
        public void OrderTest()
        {
            Assert.AreEqual(3, _queue.Count);
            using(var itr = _queue.GetEnumerator())
            {
                itr.MoveNext();
                Assert.AreEqual ("important", itr.Current);
                itr.MoveNext();
                Assert.AreEqual ("normal", itr.Current);
                itr.MoveNext();
                Assert.AreEqual ("last", itr.Current);
            }
        }
       
        [Test]
        public void RemoveTest()
        {
            _queue.Remove("normal");

            Assert.AreEqual(2, _queue.Count);
            using(var itr = _queue.GetEnumerator())
            {
                itr.MoveNext();
                Assert.AreEqual("important", itr.Current);
                itr.MoveNext();
                Assert.AreEqual("last", itr.Current);
            }
        }

        [Test]
        public void CopyTest()
        {
            var queue = new PriorityQueue<int, string>(_queue);
            Assert.AreEqual(3, queue.Count);
            using(var itr = queue.GetEnumerator())
            {
                itr.MoveNext();
                Assert.AreEqual ("important", itr.Current);
                itr.MoveNext();
                Assert.AreEqual ("normal", itr.Current);
                itr.MoveNext();
                Assert.AreEqual ("last", itr.Current);
            }
        }

        [Test]
        public void ActionTest()
        {
            bool tested = false;
            Action test = () => {
                tested = true;
            };

            var action = new PriorityAction();
            action.Add(0, () => {
                action.Add(-10, test);
                tested = false;
            });

            action.Run();
            Assert.IsFalse(tested);

            action.Run();
            Assert.IsTrue(tested);        
        }

        [Test]
        public void ActionRemoveTest()
        {
            Action test = null;

            var action = new PriorityAction();
            action.Add(0, () => {
                action.Add(10, test);
            });

            action.Add(-10, () => {
                var result = action.Remove(test);
                Assert.IsTrue(result);
            });

            action.Run();
            Assert.AreEqual(2, action.Count);
        }

    }
}
