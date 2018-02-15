using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class ParticleStopperEffect : TriggerEffect
    {
        const string kOnAnimationTriggeredMessage = "OnAnimationTriggered";

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
            CopyActionValues((ParticleStopperEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            _particles = ((ParticleStopperEffect)other).Particles;
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

        static void StopParticle(GameObject go)
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
