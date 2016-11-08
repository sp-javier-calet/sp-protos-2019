using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Class that spawns a particle effect
    // Notes: By default this class will instanciate the prefab by using GameObject.Instantiate
    [System.Serializable]
    public sealed class ParticlePlayerEffect : TriggerEffect
    {
        public interface ISpawner
        {
            GameObject Spawn(GameObject prefab);
        }

        public sealed class DefaultSpawner : ISpawner
        {
            public GameObject Spawn(GameObject prefab)
            {
                return Object.Instantiate(prefab);
            }
        }

        const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";

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
        List<GameObject> _particles = new List<GameObject>();

        public List<GameObject> Particles
        {
            get
            {
                return _particles;
            }
        }

        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyActionValues((ParticleSpawnerEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _particles = ((ParticlePlayerEffect)other).Particles;
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

            for(int i = 0; i < Particles.Count; ++i)
            {
                PlayParticle(Particles[i]);
            }
        }

        static void PlayParticle(GameObject go)
        {
            ParticleSystem particle = GUIAnimationUtility.GetComponentRecursiveDown<ParticleSystem>(go);
            if(particle != null)
            {
                particle.Play();
            }
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            Log.w(GetType() + " -> SaveValues. Nothing to save :(");
        }
    }
}
