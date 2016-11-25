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
        public bool Enabled { get { return true; } set { } }

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
        [SerializeField]
        bool _enabled;

        ModuleType _type;


        public bool Enabled
        {
            get
            {
                return _enabled;
            } 
            set
            {
                _enabled = value;
            }
        }

        public ModuleType Type
        {
            get
            {
                return _type;
            } 
            set
            {
                _type = value;
            }
        }

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
