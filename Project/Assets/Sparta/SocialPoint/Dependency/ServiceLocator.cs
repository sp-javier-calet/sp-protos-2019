using System;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.Dependency
{
    public interface IInitializable
    {
        void Initialize();
    }

    public class Binding<F>
    {
        public void ToSingle<T>() where T : F
        {
        }

        public void ToSingleInstance<T>(T instance) where T : F
        {
        }

        public void ToLookup<T>() where T : F
        {
        }

        public void ToSingleGameObject<T>() where T : MonoBehaviour, F
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
        }

        public Binding<T> Bind<T>(string tag)
        {
        }

        public void BindInstance<T>(string tag, T instance)
        {
        }

        public Binding<T> Rebind<T>()
        {
        }

        public Binding<T> Rebind<T>(string tag)
        {
        }

        public bool HasBinding<T>()
        {
        }

        public bool HasInstalled<T>() where T : IInstaller
        {
        }

        public void Install(IInstaller installer)
        {
        }

        public void Install<T>() where T : IInstaller
        {
            Install(default(T));
        }

        public T Resolve<T>()
        {
        }

        public T TryResolve<T>()
        {
        }

        public T Resolve<T>(string tag)
        {
        }

        public T TryResolve<T>(string tag, T def=default(T))
        {
        }
    }
}
