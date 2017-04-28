using NUnit.Framework;
using System.Collections.Generic;
using System;

namespace SocialPoint.Base
{
    [TestFixture]
    [Category("SocialPoint.Base")]
    public sealed class IEnumerableExtensionsTests
    {
        [Test]
        public void FirstOrDefault()
        {
            var list = new List<int>{ 1, 2, 3, 4, 5 };

            Assert.That(list.FirstOrDefault(n => n > 10), Is.EqualTo(default(int)));
            Assert.That(list.FirstOrDefault(n => n < 10), Is.EqualTo(1));
            Assert.That(list.FirstOrDefault(n => n < 5 && n > 3), Is.EqualTo(4));
            list = new List<int>();

            Assert.That(list.FirstOrDefault(n => n > 10), Is.EqualTo(default(int)));
        }

        [Test]
        public void SequenceEqual()
        {
            var list = new List<int>{ 1, 2, 3 };

            Assert.That(list.SequenceEqual(list), Is.True);
            Assert.That(list.SequenceEqual(new List<int>{ 1, 2, 3 }), Is.True);
            Assert.That(new List<int>().SequenceEqual(new List<int>()), Is.True);
            Assert.That(new List<int>{ 1, 2, 3, 4 }.SequenceEqual(list), Is.False);
            Assert.That(new List<int>{ 1, 2 }.SequenceEqual(list), Is.False);
            Assert.That(list.SequenceEqual(new List<int>{ 1, 2 }), Is.False);
            Assert.That(list.SequenceEqual(new List<int>{ 1, 2, 3, 4 }), Is.False);
            Assert.That(list.SequenceEqual(new List<int>{ 1, 3, 2 }), Is.False);
        }

        [Test]
        public void First()
        {
            Assert.That(new List<int>{ 1, 2 }.First(), Is.EqualTo(1));
            Assert.That(new List<int>().First(), Is.EqualTo(default(int)));
        }

        [Test]
        public void Where()
        {
            var list = new List<int>{ 1, 2, 3 };
            Assert.That(list.Where(n => n < 2), Is.EqualTo(new List<int>{ 1 }));
            Assert.That(list.Where(n => n > 1000), Is.EqualTo(new List<int>()));
            Assert.That(new List<int>().Where(n => n > 1000), Is.EqualTo(new List<int>()));
        }

        [Test]
        public void Contains()
        {
            var list = new []{ 1, 2, 3 };
            Assert.That(list.Contains(1), Is.True);
            Assert.That(list.Contains(1000), Is.False);
            Assert.That(new int[0].Contains(1), Is.False);
        }

        [Test]
        public void ToList()
        {
            var list = new []{ 1, 2, 3 };
            Assert.That(list.ToList(), Is.EqualTo(new List<int>{ 1, 2, 3 }));
            Assert.That(new int[0].ToList(), Is.EqualTo(new List<int>()));
        }

        [Test]
        public void ElementAt()
        {
            var list = new List<int>{ 1, 2, 3 };
            Assert.That(list.ElementAt(2), Is.EqualTo(3));
            Assert.That(new int[0].ToList(), Is.EqualTo(new List<int>()));
        }

        static void ElementAt_IndexOutOfRangeExceptionDelegate()
        {
            var list = new List<int>{ 1, 2, 3 };
            list.ElementAt(3);
        }

        #if UNITY_5_6_OR_NEWER
        [Test]
        public void ElementAt_IndexOutOfRangeException()
        {
            Assert.Throws(typeof(IndexOutOfRangeException), ElementAt_IndexOutOfRangeExceptionDelegate);
        }
        #else
        [ExpectedException(typeof(IndexOutOfRangeException))]
        [Test]
        public void ElementAt_IndexOutOfRangeException()
        {
            ElementAt_IndexOutOfRangeExceptionDelegate();
        }
        #endif

        [Test]
        public void Select()
        {
            var list = new List<int>{ 1, 2, 3 };
            Assert.That(list.Select(n => n % 2 == 0 ? "e" : "o"), Is.EqualTo(new List<string>{ "o", "e", "o" }));
        }

        [Test]
        public void Aggregate()
        {
            var list = new List<int>{ 1, 2, 3, 10, 4 };
            Assert.That(list.Aggregate(Math.Max), Is.EqualTo(10));
        }

        static void Aggregate_no_elementsDelegate()
        {
            var list = new List<int>();
            list.Aggregate(Math.Max);
        }

        #if UNITY_5_6_OR_NEWER
        [Test]
        public void Aggregate_no_elements()
        {
            Assert.Throws(typeof(Exception), Aggregate_no_elementsDelegate);
        }
        #else
        [ExpectedException(typeof(Exception))]
        [Test]
        public void Aggregate_no_elements()
        {
            Aggregate_no_elementsDelegate();
        }
        #endif
    }
}