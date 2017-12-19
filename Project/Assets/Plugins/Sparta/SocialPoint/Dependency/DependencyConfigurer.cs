using UnityEngine;

namespace SocialPoint.Dependency
{
    public sealed class DependencyConfigurer : MonoBehaviour
    {
        [SerializeField]
        public Installer[] Installers;

        void Awake()
        {
            Services.Instance.Install(Installers);
        }

        void Start()
        {
            Services.Instance.Initialize();
        }
    }
}