using UnityEngine;

namespace SocialPoint.Dependency
{
    public sealed class DependencyConfigurer : MonoBehaviour
    {
        public Installer[] Installers;

        void Awake()
        {
            ServiceLocator.Instance.Install(Installers);
        }

        void Start()
        {
            ServiceLocator.Instance.Initialize();
        }
    }
}