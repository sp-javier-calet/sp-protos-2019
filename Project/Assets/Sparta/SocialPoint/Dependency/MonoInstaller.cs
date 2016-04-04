using UnityEngine;

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        ServiceLocator Container { set; }

        void InstallBindings();
    }

    public class Installer : IInstaller
    {
        public ServiceLocator Container{ get; set; }

        public virtual void InstallBindings()
        {
        }
    }

    public class MonoInstaller : MonoBehaviour, IInstaller
    {
        public ServiceLocator Container{ get; set; }

        public virtual void InstallBindings()
        {
        }
    }

    [System.Diagnostics.DebuggerStepThrough]
    public sealed class GlobalInstallerConfig : ScriptableObject
    {
        public MonoInstaller[] Installers;
    }


    public interface IFactory<P,T>
    {
        T Create(P arg);
    }
}
