using UnityEngine;
using System;
using System.Reflection;

namespace SocialPoint.Dependency
{
    public class GlobalScriptableConfigurer : ScriptableInstaller
    {
        public ScriptableObject[] installers;

        public GlobalScriptableConfigurer() : base(ModuleType.Configurer)
        {
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void Load()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    if(t.BaseType == typeof(ScriptableInstaller))
                    {
                        InstallerAssetsManager.Create(t);
                    }
                }
            }

            installers = InstallerAssetsManager.Installers;
        }

        public override void InstallBindings()
        {
            
        }
    }
}