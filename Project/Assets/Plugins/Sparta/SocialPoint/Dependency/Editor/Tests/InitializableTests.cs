using NUnit.Framework;
using System;

namespace SocialPoint.Dependency
{
    class TestInitializable : IInitializable
    {
        public static int Count = 0;

        public void Initialize()
        {
            Count++;
        }
    }


    [TestFixture]
    [Category("SocialPoint.Dependency")]
    internal class InitializableTests
    {
        [Test]
        public void SingleResolveTest()
        {
            TestInitializable.Count = 0;
            var container = new DependencyContainer();
            var initializables = new InitializableManager(container);
            container.Bind<IInitializable>().ToSingle<TestInitializable>();
            initializables.Initialize();
            Assert.AreEqual(1, TestInitializable.Count);
            initializables.Initialize();
            Assert.AreEqual(1, TestInitializable.Count);
            container.Bind<IInitializable>().ToSingle<TestInitializable>();
            container.Bind<IInitializable>().ToSingle<TestInitializable>();
            Assert.AreEqual(1, TestInitializable.Count);
            initializables.Initialize();
            Assert.AreEqual(3, TestInitializable.Count);
        }
    }
}