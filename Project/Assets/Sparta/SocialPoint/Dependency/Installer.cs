using UnityEngine;

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        DependencyContainer Container { set; }

        void InstallBindings();
    }

    public abstract class Installer : IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }

    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }
}
