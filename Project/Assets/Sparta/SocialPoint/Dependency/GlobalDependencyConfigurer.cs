using System;

namespace SocialPoint.Dependency
{
    public sealed class GlobalDependencyConfigurer : Installer
    {
        [UnityEngine.SerializeField]
        public Installer[] Installers;

        public GlobalDependencyConfigurer() : base(ModuleType.Configurer)
        {
        }

        public override void InstallBindings()
        {

        }
    }
}