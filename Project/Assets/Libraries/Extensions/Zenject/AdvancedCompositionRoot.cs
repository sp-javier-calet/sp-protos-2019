using ModestTree;

using System;
using System.Collections.Generic;
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
                rootContainer.Install(GlobalRootInstallers);
            }
        }
    }
}
