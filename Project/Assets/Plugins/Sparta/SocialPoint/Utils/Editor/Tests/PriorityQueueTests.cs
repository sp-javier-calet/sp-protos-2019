using System;
using System.Collections;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    class PriorityQueueTests
    {
        PriorityQueue<int, string> _queue;

        public enum Priority {c = -100, b = 0, a = 100};
        ArrayList list;

        [SetUp]
        public void SetUp()
        {
            _queue = new PriorityQueue<int, string>();
            list = new ArrayList();
            foreach(Priority prio in Enum.GetValues(typeof(Priority)))
            {
                _queue.Add((int)prio,prio.ToString());
                list.Add(prio.ToString());
            }
            list.Sort();
        }

        [Test]
        public void Order()
        {
            Assert.AreEqual(3, _queue.Count);
            var testItr = list.GetEnumerator();
            using(var itr = _queue.GetEnumerator())
            {
                while(testItr.MoveNext())
                {
                    itr.MoveNext();
                    Assert.AreEqual(testItr.Current, itr.Current);
                }
            }
        }

        [Theory, Pairwise]
        public void Add(Priority prio1, Priority prio2, Priority prio3)
        {
            list.Add(prio1.ToString());
            list.Add(prio2.ToString());
            list.Add(prio3.ToString());
            list.Sort();
            _queue.Add((int)prio1,((Priority)prio1).ToString());
            _queue.Add((int)prio2,((Priority)prio2).ToString());
            _queue.Add((int)prio3,((Priority)prio3).ToString());

            var testItr = list.GetEnumerator();
            using(var itr = _queue.GetEnumerator())
            {
                while(testItr.MoveNext())
                {
                    itr.MoveNext();
                    Assert.AreEqual(testItr.Current, itr.Current);
                }
            }
        }

        [Theory, Pairwise]
        public void Remove_most_prio(Priority prio1, Priority prio2, Priority prio3)
        {
            list.Add(prio1.ToString());
            list.Add(prio2.ToString());
            list.Add(prio3.ToString());
            list.Sort();
            _queue.Add((int)prio1,((Priority)prio1).ToString());
            _queue.Add((int)prio2,((Priority)prio2).ToString());
            _queue.Add((int)prio3,((Priority)prio3).ToString());

            list.RemoveAt(0);
            _queue.Remove();

            var testItr = list.GetEnumerator();
            using(var itr = _queue.GetEnumerator())
            {
                while(testItr.MoveNext())
                {
                    itr.MoveNext();
                    Assert.AreEqual(testItr.Current, itr.Current);
                }
            }
        }

        [Theory, Pairwise]
        public void Remove_by_value(Priority prio)
        {
            list.Remove(prio.ToString());

            _queue.Remove(prio.ToString());

            Assert.AreEqual(list.Count, _queue.Count);
            var testItr = list.GetEnumerator();
            using(var itr = _queue.GetEnumerator())
            {
                while(testItr.MoveNext())
                {
                    itr.MoveNext();
                    Assert.AreEqual(testItr.Current, itr.Current);
                }
            }
        }

        [Test]
        public void Copy()
        {
            var queue = new PriorityQueue<int, string>(_queue);
            Assert.AreEqual(_queue.Count, queue.Count);
            using(var _itr = _queue.GetEnumerator())
            {
                using(var itr = queue.GetEnumerator())
                {
                    while(_itr.MoveNext())
                    {
                        itr.MoveNext();
                        Assert.AreEqual(_itr.Current, itr.Current);
                    }
                }
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
            action.Add(0, () => action.Add(10, test));

            action.Add(-10, () => {
                var result = action.Remove(test);
                Assert.IsTrue(result);
            });

            action.Run();
            Assert.AreEqual(2, action.Count);
        }

    }
}
