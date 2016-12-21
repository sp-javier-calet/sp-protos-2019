using UnityEngine;

namespace SocialPoint.Dependency
{
    public sealed class DependencyConfigurer : MonoBehaviour
    {
        [SerializeField]
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