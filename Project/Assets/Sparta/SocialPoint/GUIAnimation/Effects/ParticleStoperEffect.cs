using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
    // Class that spawns a particle effect
    // Notes: By default this class will instanciate the prefab by using GameObject.Instantiate
    [System.Serializable]
    public sealed class ParticleStoperEffect : TriggerEffect
    {
        public interface ISpawner
        {
            GameObject Spawn(GameObject prefab);
        }

        public class DefaultSpawner : ISpawner
        {
            public GameObject Spawn(GameObject prefab)
            {
                return GameObject.Instantiate(prefab);
            }
        }

        const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";

        static ISpawner _spawner = null;

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
            _particles = ((ParticleStoperEffect)other).Particles;
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
                StopParticle(Particles[i]);
            }
        }

        void StopParticle(GameObject go)
        {
            ParticleSystem particle = GUIAnimationUtility.GetComponentRecursiveDown<ParticleSystem>(go);
            if(particle != null)
            {
                particle.Stop();
            }
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            Debug.LogWarning(GetType() + " -> SaveValues. Nothing to save :(");
        }
    }
}
