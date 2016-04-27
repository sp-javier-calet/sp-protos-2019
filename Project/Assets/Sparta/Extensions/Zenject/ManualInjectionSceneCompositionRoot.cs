using ModestTree;
using UnityEngine;
using System.Collections.Generic;

namespace Zenject
{
    /**
     * It only injects the MonoBehaviours listed in MonoBehavioursToInject
     */
    public class ManualInjectionSceneCompositionRoot : SceneCompositionRoot
    {
        [SerializeField]
        public MonoBehaviour[] MonoBehavioursToInject = new MonoBehaviour[0];

        protected override void InjectObjectsInScene()
        {
            Log.Debug("Injecting registered monobehaviours in scene '{0}'", this.gameObject.scene.name);
            for(int i = 0; i < MonoBehavioursToInject.Length; ++i)
            {
                Container.Inject(MonoBehavioursToInject[i]);

            }
        }
    }
}
