using System;
using UnityEngine;

namespace SocialPoint.Dependency
{
    [UnityEngine.CreateAssetMenu(fileName = "GlobalDependencyConfigurer", menuName = "Sparta/Global Dependency Configurer")]
    public sealed class GlobalDependencyConfigurer : Installer
    {
        const string ResourcePath = "Installers/GlobalDependencyConfigurer";

        public static GlobalDependencyConfigurer Load()
        {
            return Resources.Load<GlobalDependencyConfigurer>(ResourcePath);
        }

        [SerializeField]
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