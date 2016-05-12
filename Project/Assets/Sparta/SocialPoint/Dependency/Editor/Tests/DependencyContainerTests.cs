
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
    internal class DependencyContainerTests
    {


        [Test]
        public void SingleResolveTest()
        {       
            var container = new DependencyContainer();
            container.Bind<ITestService>().ToSingle<TestService>();
            var service = container.Resolve<ITestService>();
            Assert.AreEqual("test", service.TestMethod());
            var service2 = container.Resolve<ITestService>();
            Assert.AreEqual(service, service2);
        }

        [Test]
        public void SingleMethodResolveTest()
        {       
            var container = new DependencyContainer();
            container.Bind<ITestService>().ToSingle<TestService>();
            container.Bind<DependentService>().ToMethod<DependentService>(() => {
                return new DependentService(container.Resolve<ITestService>());
            });
            var service = container.Resolve<DependentService>();
            Assert.AreEqual("test", service.TestMethod());
        }

        [Test]
        public void LookupResolveTest()
        {
            var container = new DependencyContainer();
            container.Bind<TestService>().ToSingle<TestService>();
            container.Bind<IDisposable>().ToLookup<TestService>();
            var service = container.Resolve<TestService>();
            var disposable = container.Resolve<IDisposable>();
            Assert.AreEqual(service, disposable);
        }

        [Test]
        public void InstanceResolveTest()
        {
            var container = new DependencyContainer();
            var instance = new TestService();
            container.Bind<ITestService>().ToInstance(instance);
            var service = container.Resolve<ITestService>();
            Assert.AreEqual(instance, service);
        }

        [Test]
        public void ResolveLoopTest()
        {
            var container = new DependencyContainer();
            container.Bind<ITestService>().ToLookup<ITestService>();
            var service = container.Resolve<ITestService>();
            Assert.IsNull(service);
        }

        [Test]
        public void ResolveDoubleLoopTest()
        {
            var container = new DependencyContainer();
            container.Bind<ITestService>().ToMethod(() => {
                return new LoopTestService(container.Resolve<DependentService>());
            });
            container.Bind<DependentService>().ToMethod<DependentService>(() => {
                return new DependentService(container.Resolve<ITestService>());
            });
            Assert.Throws<InvalidOperationException>(() => {
                container.Resolve<ITestService>();
            });
        }
    }
}
