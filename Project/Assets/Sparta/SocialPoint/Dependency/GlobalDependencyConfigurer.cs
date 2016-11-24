using System;

namespace SocialPoint.Dependency
{
    public sealed class GlobalDependencyConfigurer : Installer
    {
        [UnityEngine.SerializeField]
        public Installer[] Installers;

        public GlobalDependencyConfigurer() : base(ModuleType.Configurer)
        {
            Enabled = true;
        }

        public override void InstallBindings()
        {
            for(var i = 0; i < Installers.Length; ++i)
            {
                var installer = Installers[i];
                Container.Install(installer);
            }
        }
    }
}