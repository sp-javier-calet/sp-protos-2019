using System;

namespace SocialPoint.Dependency
{
    public sealed class GlobalDependencyConfigurer : Installer
    {
        [UnityEngine.SerializeField]
        public Installer[] Installers;

        public GlobalDependencyConfigurer() : base(ModuleType.Configurer)
        {
            IsGlobal = true;
        }

        public override void InstallBindings()
        {
            for(var i = 0; i < Installers.Length; ++i)
            {
                var installer = Installers[i];
                if(installer.IsGlobal)
                {
                    Container.Install(installer);
                }
            }
        }
    }
}