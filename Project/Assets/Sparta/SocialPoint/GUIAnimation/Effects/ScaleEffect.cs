using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class ScaleEffect : BlendEffect
    {
        [System.Serializable]
        public class TargetValueMonitor : StepMonitor
        {
            public Vector3 Scale;

            public override void Backup()
            {
                Scale = Target.localScale;
            }

            public override bool HasChanged()
            {
                return Scale != Target.localScale;
            }
        }

        [SerializeField]
        Vector3 _startValue = Vector3.one;

        public Vector3 StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        Vector3 _endValue = Vector3.one;

        public Vector3 EndValue { get { return _endValue; } set { _endValue = value; } }

        public override void Copy(Step other)
        {
            base.Copy(other);

            SetOrCreateDefaultValues();

            CopyActionValues((ScaleEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            CopyValues(ref _startValue, ((ScaleEffect)other).StartValue);
            CopyValues(ref _endValue, ((ScaleEffect)other).EndValue);
        }

        public void RemoveAnchors()
        {
        }

        public void SetAnchors()
        {
        }

        public override void SetOrCreateDefaultValues()
        {
            if(Target != null)
            {
                SaveValuesAt(0f);
                SaveValuesAt(1f);
            }
        }

        static void CopyValues(ref Vector3 dest, Vector3 src)
        {
            dest = src;
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            Vector3 endScale = _endValue;
            Vector3 startScale = _startValue;

            _startValue = endScale;
            _endValue = startScale;
        }

        public override void OnRemoved()
        {
        }

        public override void OnBlend(float blend)
        {
            if(Target == null)
            {
                if(Animation != null && Animation.EnableWarnings)
                {
                    Log.w(GetType() + " OnBlend " + StepName + " Target is null");
                }
                return;
            }

            Target.localScale = Vector3.LerpUnclamped(_startValue, _endValue, blend);
        }

        public override void SaveValues()
        {
            StartValue = Target.localScale;
            EndValue = Target.localScale;
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            if(Target == null)
            {
                if(Animation != null && Animation.EnableWarnings)
                {
                    Log.w(GetType() + " Target is null");
                }
                return;
            }

            if(localTimeNormalized < 0.5f)
            {
                StartValue = Target.localScale;
            }
            else
            {
                EndValue = Target.localScale;
            }
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}
