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
            installers = ScriptableInstallerManager.Installers;
            // TODO Load all assets under config/installers
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach(var t in assembly.GetTypes())
                {
                    if(t.BaseType == typeof(ScriptableInstaller))
                    {
                        ScriptableInstallerManager.Create(t);
                    }
                }
            }

            installers = ScriptableInstallerManager.Installers;
        }

        public override void InstallBindings()
        {
            
        }
    }
}