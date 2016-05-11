using UnityEngine;

namespace SocialPoint.Dependency
{
    public sealed class DependencyConfigurer : MonoBehaviour
    {
        public MonoInstaller[] Installers;

        void Awake()
        {
            ServiceLocator.Instance.Install(Installers);
        }
    }
}