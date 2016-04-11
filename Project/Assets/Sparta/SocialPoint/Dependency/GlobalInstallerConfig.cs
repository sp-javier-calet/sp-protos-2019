using UnityEngine;

namespace SocialPoint.Dependency
{
    [System.Diagnostics.DebuggerStepThrough]
    public sealed class GlobalInstallerConfig : ScriptableObject
    {
        public MonoInstaller[] Installers;
    }
}