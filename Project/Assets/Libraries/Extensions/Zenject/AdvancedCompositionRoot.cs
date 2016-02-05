using ModestTree;
using UnityEngine;

namespace Zenject
{
    /**
     * For this to work, go to Edit -> Projects Settings -> Script Execution Order
     * and add Zenject.AdvancedCompositionRoot before Zenject.CompositionRoot
     */
    public sealed class AdvancedCompositionRoot : MonoBehaviour
    {
        [SerializeField]
        public MonoInstaller[] GlobalRootInstallers = new MonoInstaller[0];

        public void Awake()
        {
            var rootContainer = GlobalCompositionRoot.Instance.Container;
            if(rootContainer != null)
            {
                foreach(var installer in GlobalRootInstallers)
                {
                    if(!rootContainer.HasInstalled(installer.GetType()))
                    {
                        rootContainer.Install(installer);
                    }
                }
            }
        }
    }
}
