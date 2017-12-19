using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public abstract class BlendEffect : Effect, IBlendeableEffect
    {
        [SerializeField]
        bool _useEaseCustom;

        public bool UseEaseCustom { get { return _useEaseCustom; } set { _useEaseCustom = value; } }

        [SerializeField]
        List<EasePoint> _easeCustom = new List<EasePoint> {
            new EasePoint(0f, 0f),
            new EasePoint(1f, 1f)
        };

        public List<EasePoint> EaseCustom { get { return _easeCustom; } set { _easeCustom = value; } }

        [SerializeField]
        EaseType _easeType;

        public EaseType EaseType { get { return _easeType; } set { _easeType = value; } }

        public void CopyEasing(bool useEaseCustom, List<EasePoint> easeCustom, EaseType easeType)
        {
            _useEaseCustom = useEaseCustom;
            _easeCustom = new List<EasePoint>(easeCustom);
            _easeType = easeType;
        }

        public override void Copy(Step other)
        {
            base.Copy(other);
            CopyEasing(((BlendEffect)other).UseEaseCustom, ((BlendEffect)other).EaseCustom, ((BlendEffect)other).EaseType);
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);
            Easing.InvertCustom(_easeCustom);
        }

        public override void OnUpdate()
        {
            if(IsEnabledInHierarchy())
            {
                float actionStartTime = GetStartTime(AnimTimeMode.Global);
                float actionEndTime = GetEndTime(AnimTimeMode.Global);
				
                float t = _animation.CurrentTime;
                float prevT = _animation.PrevTime;

                if(t >= actionStartTime && prevT < actionEndTime)
                {
                    float delta = Mathf.Min(t, actionEndTime) - actionStartTime;
                    float duration = actionEndTime - actionStartTime;

                    float blend = GetBlendValue(delta, 0f, 1f, duration);
					
                    OnBlend(blend);
                }
            }
        }

        protected float GetStartBlendValue()
        {
            return GetBlendValue(0f, 0f, 1f, 1f);
        }

        protected float GetEndBlendValue()
        {
            return GetBlendValue(1f, 0f, 1f, 1f);
        }

        float GetBlendValue(float time, float start, float deltaVal, float duration)
        {
            return _useEaseCustom ? Easing.Custom(time, duration, _easeCustom) : _easeType.ToFunction()(time, start, deltaVal, duration);
        }

        public abstract void OnBlend(float blend);

        public override void OnReset()
        {
            OnBlend(0f);
        }
    }
}
