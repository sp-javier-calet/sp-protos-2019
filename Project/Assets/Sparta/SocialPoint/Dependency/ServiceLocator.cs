using System;
using SocialPoint.Utils;

namespace SocialPoint.Dependency
{
    public interface IInitializable
    {
        void Initialize();
    }

    public class Binding<F>
    {
        public Binding(ServiceLocator container)
        {
        }

        public void ToSingle()
        {
        }

        public void ToSingle<T>() where T : F
        {
        }

        public void ToSingleInstance<T>(T instance) where T : F
        {
        }

        public void ToLookup<T>() where T : F
        {
        }

        public void ToSingleMethod<T>(Func<T> method) where T : F
        {
        }

        public void ToGetter<T>(Func<T,F> method) where T : F
        {
        }
    }

    public class ServiceLocator : MonoBehaviourSingleton<ServiceLocator>
    {

        public Binding<T> Bind<T>()
        {
            return new Binding<T>(this);
        }

        public Binding<T> Bind<T>(string tag)
        {
            return new Binding<T>(this);
        }

        public void BindInstance<T>(string tag, T instance)
        {
        }

        public Binding<T> Rebind<T>()
        {
            return new Binding<T>(this);
        }

        public Binding<T> Rebind<T>(string tag)
        {
            return new Binding<T>(this);
        }

        public bool HasBinding<T>()
        {
            return false;
        }

        public bool HasInstalled<T>() where T : IInstaller
        {
            return false;
        }

        public void Install(IInstaller installer)
        {
            installer.Container = this;
            installer.InstallBindings();
        }

        public void Install<T>() where T : IInstaller
        {
            Install(default(T));
        }

        public T Resolve<T>()
        {
            return default(T);
        }

        public T TryResolve<T>()
        {
            return default(T);
        }

        public T Resolve<T>(string tag)
        {
            return default(T);
        }

        public T TryResolve<T>(string tag, T def=default(T))
        {
            return default(T);
        }
    }
}
