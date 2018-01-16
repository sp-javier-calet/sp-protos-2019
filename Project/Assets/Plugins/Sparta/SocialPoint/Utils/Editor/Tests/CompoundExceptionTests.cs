using System;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

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
    public class CompoundExceptionTests
    {
        const string _testStack = @"first line
second line";

        static Exception[] GetExceptions()
        {
            return new Exception[] {
                new Exception("outer1", new TestException("inner1", _testStack, new Exception("inner2"))),
                new TestException("outer2", _testStack, new Exception("inner1"))
            };
        }

        [Test]
        public void ConvertToString()
        {
            var str = CompoundException.GetString(GetExceptions());
            Assert.AreEqual(@"SocialPoint.Utils.CompoundException: Multiple Exceptions thrown:
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

        [Test]
        public void TriggerLog()
        {
            var logCount = 0;
            Action<Exception> onTriggered = (ex) => {
                logCount++;
            };
            CompoundException.Triggered += onTriggered;

            Assert.DoesNotThrow(() => {
                CompoundException.Trigger(GetExceptions());
            });
            try
            {
                Assert.AreEqual(2, logCount);
            }
            finally
            {
                CompoundException.Triggered -= onTriggered;
            }
        }

        [Test]
        public void TriggerThrow()
        {
            Assert.Throws<CompoundException>(() => {
                CompoundException.Trigger(GetExceptions());
            });
        }
    }
}
