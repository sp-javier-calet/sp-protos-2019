using NSubstitute;
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

    class AnotherTestService : ITestService
    {
        public string TestMethod()
        {
            return "anotherTest";
        }

        public void Dispose()
        {
        }
    }

    struct TestStruct
    {
        public int Value;
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

    class TestBinding : IBinding
    {
        public BindingKey Key
        {
            get
            {
                return new BindingKey(typeof(TestBinding), null);
            }
        }

        public object Resolve()
        {
            return new TestService();
        }

        public bool Resolved
        {
            get
            {
                return false;
            }
        }

        public void OnResolved()
        {
        }
    }

    class TestDisposable : IDisposable
    {
        public static int Count = 0;

        public void Dispose()
        {
            Count++;
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
        public void TagResolveTest()
        {       
            var container = new DependencyContainer();
            container.Bind<string>("tag1").ToInstance<string>("value1");
            container.Bind<string>("tag2").ToInstance<string>("value2");
            var one = container.Resolve<string>("tag1");
            var two = container.Resolve<string>("tag2");
            Assert.AreEqual("value1", one);
            Assert.AreEqual("value2", two);
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
        public void ResolveAddedInstanceTest()
        {
            var container = new DependencyContainer();
            var instance = new TestService();
            container.Add<ITestService, TestService>(instance);
            var service = container.Resolve<ITestService>();
            Assert.AreEqual(instance, service);
        }

        [Test]
        public void ResolveListTest()
        {
            var container = new DependencyContainer();
            var instance = new TestService();
            container.Bind<ITestService>().ToInstance(instance);
            container.Bind<ITestService>().ToInstance(instance);
            var services = container.ResolveList<ITestService>();
            Assert.AreEqual(2, services.Count);
        }

        [Test]
        public void RebindTest()
        {
            var container = new DependencyContainer();
            var instance = new TestService();
            container.Bind<ITestService>().ToInstance(instance);
            container.Rebind<ITestService>().ToInstance(instance);
            var services = container.ResolveList<ITestService>();
            Assert.AreEqual(1, services.Count);
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

        [Test]
        public void ValueTypeResolveSingleTest()
        {       
            var container = new DependencyContainer();
            container.Bind<TestStruct>().ToSingle<TestStruct>();
            var resolved = container.Resolve<TestStruct>();
            Assert.AreEqual(0, resolved.Value);
        }

        [Test]
        public void ValueTypeResolveInstanceTest()
        {       
            var container = new DependencyContainer();
            const int value = 10;
            var instance = new TestStruct();
            instance.Value = value;
            container.Bind<TestStruct>().ToInstance<TestStruct>(instance);
            var resolved = container.Resolve<TestStruct>();
            Assert.AreEqual(value, resolved.Value);
        }

        [Test]
        public void ValueTypeResolveMethodTest()
        {       
            var container = new DependencyContainer();
            const int value = 10;
            container.Bind<TestStruct>().ToMethod<TestStruct>(
                () => {
                    var instance = new TestStruct();
                    instance.Value = value;
                    return instance;
                });
            var resolved = container.Resolve<TestStruct>();
            Assert.AreEqual(value, resolved.Value);
        }

        [Test]
        public void ValueTypeResolveGetterTest()
        {       
            var container = new DependencyContainer();
            const int value = 10;
            var instance = new TestStruct();
            instance.Value = value;
            container.Bind<TestStruct>().ToInstance<TestStruct>(instance);
            container.Bind<int>().ToGetter<TestStruct>(
                (x) => {
                    return x.Value;
                });
            var resolved = container.Resolve<int>();
            Assert.AreEqual(value, resolved);
        }

        [Test]
        public void ValueTypeResolveLookupTest()
        {       
            var container = new DependencyContainer();
            const int value = 10;
            var instance = new TestStruct();
            instance.Value = value;
            container.Bind<TestStruct>("tag").ToInstance<TestStruct>(instance);
            container.Bind<TestStruct>().ToLookup<TestStruct>("tag");
            var resolved = container.Resolve<TestStruct>();
            Assert.AreEqual(value, resolved.Value);
        }

        [Test]
        public void HasBindingTest()
        {
            var container = new DependencyContainer();
            container.AddBinding(new TestBinding(), typeof(ITestService));
            Assert.IsTrue(container.HasBinding<ITestService>());
        }

        [Test]
        public void HasDefaultBindingTest()
        {
            var container = new DependencyContainer();
            var defaultTestService = new TestService();
            container.BindDefault<ITestService>().ToInstance(defaultTestService);

            Assert.IsTrue(container.HasBinding<ITestService>());
        }

        [Test]
        public void AddBindingTest()
        {
            var container = new DependencyContainer();
            container.AddBinding(new TestBinding(), typeof(ITestService));
            var service = container.Resolve<ITestService>();
            Assert.IsInstanceOf<TestService>(service);
        }

        [Test]
        public void AddDefaultBindingTest()
        {
            var container = new DependencyContainer();
            var defaultTestService = new TestService();
            container.BindDefault<ITestService>().ToInstance(defaultTestService);

            Assert.IsInstanceOf<TestService>(container.Resolve<ITestService>());

            var servicesList = container.ResolveList<ITestService>();
            foreach(var service in servicesList)
            {
                Assert.IsInstanceOf<TestService>(service);
            }
        }

        [Test]
        public void AddNormalAndDefaultBindingTest()
        {
            var container = new DependencyContainer();
            var defaultTestService = new TestService();
            var customTestService = new AnotherTestService();
            container.BindDefault<ITestService>().ToInstance(defaultTestService);
            container.Bind<ITestService>().ToInstance(customTestService);

            Assert.IsInstanceOf<AnotherTestService>(container.Resolve<ITestService>());

            var servicesList = container.ResolveList<ITestService>();
            foreach(var service in servicesList)
            {
                Assert.IsInstanceOf<AnotherTestService>(service);
            }
        }

        [Test]
        public void InstallTest()
        {
            var container = new DependencyContainer();
            Assert.IsFalse(container.HasInstalled<TestInstaller>());
            container.Install<TestInstaller>();
            Assert.IsTrue(container.HasInstalled<TestInstaller>());
            var service = container.Resolve<ITestService>();
            Assert.IsInstanceOf<TestService>(service);
        }

        [Test]
        public void DontSetupTwiceTest()
        {
            var container = new DependencyContainer();
            var count = 0;
            container.Bind<ITestService>().ToMethod<TestService>(() => {
                return new TestService();
            }, (service) => {
                count++;
            });
            container.Resolve<ITestService>();
            container.Resolve<ITestService>();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void DisposeTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            container.Bind<IDisposable>().ToSingle<TestDisposable>();
            container.Dispose();
            Assert.AreEqual(0, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Dispose();
            Assert.AreEqual(2, TestDisposable.Count);
        }

        [Test]
        public void DisposeDefaultTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            container.BindDefault<IDisposable>().ToSingle<TestDisposable>();
            container.Dispose();
            Assert.AreEqual(0, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Dispose();
            Assert.AreEqual(2, TestDisposable.Count);
        }

        [Test]
        public void DisposeLookupTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            container.Bind<TestDisposable>().ToSingle<TestDisposable>();
            container.Bind<IDisposable>().ToLookup<TestDisposable>();
            // Tagged instance shouldn't be disposed
            container.Bind<TestDisposable>("tag").ToSingle<TestDisposable>();
            container.Dispose();
            Assert.AreEqual(0, TestDisposable.Count);
            container.Resolve<TestDisposable>();
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
        }

        [Test]
        public void DisposeInstanceTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            var instance = new TestDisposable();
            container.Bind<IDisposable>().ToInstance(instance);
            container.Dispose();
            Assert.AreEqual(0, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
        }

        [Test]
        public void DisposeAddedInstanceTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            var instance = new TestDisposable();
            container.Add<IDisposable, TestDisposable>(instance);
            container.Dispose();
            Assert.AreEqual(1, TestDisposable.Count);
        }

        [Test]
        public void IgnoreRebindDisposableTest()
        {
            TestDisposable.Count = 0;
            var container = new DependencyContainer();
            container.Rebind<TestDisposable>().ToSingle<TestDisposable>();
            container.Bind<IDisposable>().ToGetter<TestDisposable>((service) => {
                return new TestDisposable();
            });

            container.Rebind<TestDisposable>().ToSingle<TestDisposable>();
            Assert.AreEqual(0, TestDisposable.Count);
            container.Resolve<IDisposable>();
            container.Rebind<TestDisposable>().ToSingle<TestDisposable>();
            Assert.AreEqual(0, TestDisposable.Count);
        }

        [Test]
        public void AddListener()
        {
            var container = new DependencyContainer();
            container.Bind<TestDisposable>().ToSingle<TestDisposable>();

            var setupCallback = Substitute.For<Action<TestDisposable>>();
            container.Listen<TestDisposable>().WhenResolved(setupCallback);
            container.Resolve<TestDisposable>();

            setupCallback.Received().Invoke(Arg.Any<TestDisposable>());
        }

        [Test]
        public void AddListenerWithLookup()
        {
            var container = new DependencyContainer();
            container.Bind<TestDisposable>().ToSingle<TestDisposable>();
            container.Bind<IDisposable>().ToLookup<TestDisposable>();

            var setupCallback = Substitute.For<Action<IDisposable>>();
            container.Listen<IDisposable>().WhenResolved(setupCallback);
            container.Resolve<TestDisposable>();

            setupCallback.Received().Invoke(Arg.Any<IDisposable>());
        }

        [Test]
        public void ListenerDontResolvedTwice()
        {
            var container = new DependencyContainer();
            container.Bind<TestDisposable>().ToSingle<TestDisposable>();

            var setupCallback = Substitute.For<Action<TestDisposable>>();
            container.Listen<TestDisposable>().WhenResolved(setupCallback);
            container.Resolve<TestDisposable>();
            container.Resolve<TestDisposable>();

            setupCallback.Received(1).Invoke(Arg.Any<TestDisposable>());
        }
    }
}
