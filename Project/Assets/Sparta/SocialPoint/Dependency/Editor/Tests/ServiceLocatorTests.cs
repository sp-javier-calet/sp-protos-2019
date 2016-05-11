
using NUnit.Framework;
using System;

namespace SocialPoint.Dependency
{
    interface ITestService : IDisposable
    {
        string TestMethod();
    }

    class TestService : ITestService
    {
        public string TestMethod()
        {
            return "test";
        }

        public void Dispose()
        {
        }
    }

    class DependentService
    {
        ITestService _test;

        public DependentService(ITestService test)
        {
            if(test == null)
            {
                throw new InvalidOperationException("Need a test service");
            }
            _test = test;
        }

        public string TestMethod()
        {
            return _test.TestMethod();
        }
    }


    class LoopTestService : ITestService
    {
        public LoopTestService(DependentService dep)
        {
        }

        public string TestMethod()
        {
            return "loop";
        }

        public void Dispose()
        {
        }
    }


    [TestFixture]
    [Category("SocialPoint.Dependency")]
    internal class ServiceLocatorTests
    {

        [SetUp]
        public void SetUp()
        {
            var locator = ServiceLocator.Instance;
            locator.Clear();
        }

        [Test]
        public void SingleResolveTest()
        {       
            var locator = ServiceLocator.Instance;
            locator.Bind<ITestService>().ToSingle<TestService>();
            var service = locator.Resolve<ITestService>();
            Assert.AreEqual("test", service.TestMethod());
            var service2 = locator.Resolve<ITestService>();
            Assert.AreEqual(service, service2);
        }

        [Test]
        public void SingleMethodResolveTest()
        {       
            var locator = ServiceLocator.Instance;
            locator.Bind<ITestService>().ToSingle<TestService>();
            locator.Bind<DependentService>().ToMethod<DependentService>(() => {
                return new DependentService(locator.Resolve<ITestService>());
            });
            var service = locator.Resolve<DependentService>();
            Assert.AreEqual("test", service.TestMethod());
        }

        [Test]
        public void LookupResolveTest()
        {
            var locator = ServiceLocator.Instance;
            locator.Bind<TestService>().ToSingle<TestService>();
            locator.Bind<IDisposable>().ToLookup<TestService>();
            var service = locator.Resolve<TestService>();
            var disposable = locator.Resolve<IDisposable>();
            Assert.AreEqual(service, disposable);
        }

        [Test]
        public void InstanceResolveTest()
        {
            var locator = ServiceLocator.Instance;
            var instance = new TestService();
            locator.Bind<ITestService>().ToInstance(instance);
            var service = locator.Resolve<ITestService>();
            Assert.AreEqual(instance, service);
        }

        [Test]
        public void ResolveLoopTest()
        {
            var locator = ServiceLocator.Instance;
            var instance = new TestService();
            locator.Bind<ITestService>().ToLookup<ITestService>();
            var service = locator.Resolve<ITestService>();
            Assert.IsNull(service);
        }

        [Test]
        public void ResolveDoubleLoopTest()
        {
            var locator = ServiceLocator.Instance;
            locator.Bind<ITestService>().ToMethod(() => {
                return new LoopTestService(locator.Resolve<DependentService>());
            });
            locator.Bind<DependentService>().ToMethod<DependentService>(() => {
                return new DependentService(locator.Resolve<ITestService>());
            });
            Assert.Throws<InvalidOperationException>(() => {
                locator.Resolve<ITestService>();
            });
        }
    }
}
