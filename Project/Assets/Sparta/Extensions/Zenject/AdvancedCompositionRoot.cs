using ModestTree;
using UnityEngine;
using System.Collections.Generic;

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
            if(rootContainer == null)
            {
                return;
            }

            var oldInits = rootContainer.TryResolve<List<IInitializable>>();
            foreach(var installer in GlobalRootInstallers)
            {
                if(!rootContainer.HasInstalled(installer.GetType()))
                {
                    rootContainer.Install(installer);
                }
            }
            var newInits = rootContainer.TryResolve<List<IInitializable>>();
            if(newInits != null)
            {
                foreach(var init in newInits)
                {
                    if(oldInits == null || !oldInits.Contains(init))
                    {
                        init.Initialize();
                    }
                }
            }
        }
    }
}
