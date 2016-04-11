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
}
