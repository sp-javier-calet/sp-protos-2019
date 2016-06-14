using UnityEngine;

namespace SocialPoint.Dependency
{
    public sealed class GlobalDependencyConfigurer : ScriptableObject
    {
        public Installer[] Installers;
    }
}