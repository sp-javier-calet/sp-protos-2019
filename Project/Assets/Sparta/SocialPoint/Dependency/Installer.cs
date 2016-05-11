using UnityEngine;

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        DependencyContainer Container { set; }

        void InstallBindings();
    }

    public class Installer : IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public virtual void InstallBindings()
        {
        }
    }

    public class MonoInstaller : MonoBehaviour, IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public virtual void InstallBindings()
        {
        }
    }
}
