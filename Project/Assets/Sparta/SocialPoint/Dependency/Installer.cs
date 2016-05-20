#if UNITY_5
using UnityEngine;
#endif

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        DependencyContainer Container { set; }

        void InstallBindings();
    }


    public abstract class SubInstaller : IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }

    public abstract class Installer : 
    #if UNITY_5
    MonoBehaviour,
    #endif
    IInstaller
    {
        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }

}
