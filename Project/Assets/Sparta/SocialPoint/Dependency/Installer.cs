#if UNITY_5
using UnityEngine;
#endif

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        bool Enabled { get; set; }

        ModuleType Type { get; }

        DependencyContainer Container { set; }

        void InstallBindings();
    }


    public abstract class SubInstaller : IInstaller
    {
        public bool Enabled { get; set; }

        public ModuleType Type { get; private set; }

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }

    public abstract class Installer : 
    #if UNITY_5
    ScriptableObject,
    #endif
    IInstaller
    {
        public bool Enabled { get; set; }

        public ModuleType Type { get; private set; }

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();

        protected Installer() : this(ModuleType.Service)
        {
        }

        protected Installer(ModuleType type)
        {
            Type = type;
        }
    }

}
