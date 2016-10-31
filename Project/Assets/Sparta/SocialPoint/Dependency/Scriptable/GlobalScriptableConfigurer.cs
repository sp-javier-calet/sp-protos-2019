using UnityEngine;

namespace SocialPoint.Dependency
{
    public class GlobalScriptableConfigurer : ScriptableInstaller<GlobalScriptableConfigurer>
    {
        public ScriptableObject[] installers;

        public GlobalScriptableConfigurer()
        {
            Type = ModuleType.Configurer;
        }

        void Load()
        {
            // TODO Load all assets under config/installers
        }

        public override void InstallBindings()
        {
            
        }
    }
}