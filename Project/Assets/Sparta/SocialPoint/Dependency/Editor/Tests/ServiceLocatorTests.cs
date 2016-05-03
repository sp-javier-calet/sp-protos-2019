
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
            _test = test;
        }

        public string TestMethod()
        {
            return _test.TestMethod();
        }
    }


    [TestFixture]
    [Category("SocialPoint.Dependency")]
    internal class ServiceLocatorTests
    {
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
            locator.Bind<DependentService>().ToSingleMethod<DependentService>(() => {
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
            locator.Bind<ITestService>().ToSingleInstance(instance);
            var service = locator.Resolve<ITestService>();
            Assert.AreEqual(instance, service);
        }

    }
}
