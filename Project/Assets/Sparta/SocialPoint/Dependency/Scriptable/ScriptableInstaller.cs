using UnityEngine;

namespace SocialPoint.Dependency
{
    public abstract class ScriptableInstaller : ScriptableObject
    {
        public bool Enabled { get; set; }
        public ModuleType Type { get; private set; }

        protected ScriptableInstaller()
        {
            Type = ModuleType.Service;
        }

        protected ScriptableInstaller(ModuleType type)
        {
            Type = type;
        }

        public DependencyContainer Container{ get; set; }

        public abstract void InstallBindings();
    }
}
