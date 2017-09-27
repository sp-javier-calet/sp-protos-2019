#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace SocialPoint.Dependency
{
    public interface IInstaller
    {
        bool IsGlobal { get; }

        ModuleType Type { get; }

        DependencyContainer Container { set; }

        void InstallBindings();
    }

    public abstract class SubInstaller : IInstaller
    {
        public bool IsGlobal { get { return true; } }

        public ModuleType Type { get; private set; }

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }

    public abstract class Installer : 
    #if UNITY_5_3_OR_NEWER
    ScriptableObject,
    #endif
    IInstaller
    {
        [SerializeField]
        bool _isGlobal;

        public bool IsGlobal
        {
            get
            {
                return _isGlobal;
            } 
            set
            {
                _isGlobal = value;
            }
        }

        public ModuleType Type { get; set; }

        public DependencyContainer Container{ get; set; }

        public bool IsDefault
        {
            get
            {
                return this.GetType().Name == name;
            }
        }

        public abstract void InstallBindings();

        protected Installer() : this(ModuleType.Game)
        {
        }

        protected Installer(ModuleType type)
        {
            Type = type;
        }
    }

    public abstract class ServiceInstaller : Installer
    {
        protected ServiceInstaller() : base(ModuleType.Service)
        {
        }
    }
}
