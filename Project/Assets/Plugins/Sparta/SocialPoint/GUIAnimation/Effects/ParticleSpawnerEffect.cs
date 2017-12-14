using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class that spawns a particle effect
    // Notes: By default this class will instanciate the prefab by using GameObject.Instantiate
    // If the client wants to use another way to spawn the particle a custom ISpawner must be setted
    // by using SetSpawner method
    [System.Serializable]
    public sealed class ParticleSpawnerEffect : TriggerEffect
    {
        public interface ISpawner
        {
            GameObject Spawn(GameObject prefab);
        }

        public class DefaultSpawner : ISpawner
        {
            public GameObject Spawn(GameObject prefab)
            {
                return Object.Instantiate(prefab);
            }
        }

        const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";
        const string kDefaultLayer = "UI";

        static ISpawner _spawner;

        static ISpawner Spawner
        {
            get
            {
                if(_spawner == null)
                {
                    _spawner = new DefaultSpawner();
                }
                return _spawner;
            }
        }

        [ShowInEditor]
        [SerializeField]
        string _layer = kDefaultLayer;

        public string Layer
        {
            get
            {
                if(string.IsNullOrEmpty(_layer))
                {
                    _layer = kDefaultLayer;
                }
				
                return _layer;
            }
        }

        [ShowInEditor]
        [SerializeField]
        bool _keepScale = true;

        [ShowInEditor]
        [SerializeField]
        List<GameObject> _particlesPrefabs = new List<GameObject>();

        public List<GameObject> ParticlesPrefabs
        {
            get
            {
                return _particlesPrefabs;
            }
        }

        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyActionValues((ParticleSpawnerEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _particlesPrefabs = ((ParticleSpawnerEffect)other).ParticlesPrefabs;
            _layer = ((ParticleSpawnerEffect)other).Layer;
        }

        public override void OnRemoved()
        {
        }

        public override void SetOrCreateDefaultValues()
        {
        }

        public override void DoAction()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            for(int i = 0; i < _particlesPrefabs.Count; ++i)
            {
                GameObject go = DoSpawnParticle(_particlesPrefabs[i]);
                SetLayer(go);
                FixScale(go);
            }
        }

        public static void SetSpawner(ISpawner spawner)
        {
            _spawner = spawner;
        }

        void FixScale(GameObject go)
        {
            if(_keepScale && Parent != null)
            {
                go.transform.localScale = new Vector3(go.transform.localScale.x / Parent.transform.lossyScale.x, go.transform.localScale.y / Parent.transform.lossyScale.y, go.transform.localScale.z / Parent.transform.lossyScale.z);
            }
        }

        GameObject DoSpawnParticle(GameObject prefab)
        {
            GameObject instance = Spawner.Spawn(prefab);
            instance.transform.SetParent(Target, false);
			
            ParticleSystem pSystem = instance.GetComponentInChildren<ParticleSystem>();
            pSystem.Play();

            instance.SendMessage(kOnAnimationTriggeredMessage, SendMessageOptions.DontRequireReceiver);
			
            return instance;
        }

        void SetLayer(GameObject go)
        {
            GameObjectUtility.SetLayerRecursively(go, LayerMask.NameToLayer(Layer));
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            Log.w(GetType() + " -> SaveValues. Nothing to save :(");
        }
    }
}
