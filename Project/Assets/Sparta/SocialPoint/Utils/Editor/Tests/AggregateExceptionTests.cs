using System;
using NSubstitute;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    class TestException : Exception
    {
        private string _stack;

        public TestException(string message, string stack, Exception inner) : base(message, inner)
        {
            _stack = stack;
        }

        public override string StackTrace
        {
            get
            {
                return _stack;
            }
        }
    }

    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class AggregateExceptionTests
    {
        [Test]
        public void ConvertToString()
        {
            var str = new AggregateException(new Exception[] {
                new Exception("outer1", new TestException("inner1", "first line\nsecond line", new Exception("inner2"))),
                new TestException("outer2", "first line\nsecond line", new Exception("inner1"))
            }).ToString();
            Assert.AreEqual(@"SocialPoint.Utils.AggregateException: Multiple Exceptions thrown:
1. Exception: outer1
    TestException: inner1
    first line
    second line
        Exception: inner2
2. TestException: outer2
first line
second line
    Exception: inner1
", str);
        }
    }
}
