using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class RotationEffect : BlendEffect
    {
        [System.Serializable]
        public class TargetValueMonitor : StepMonitor
        {
            public Quaternion Rotation;

            public override void Backup()
            {
                Rotation = Target.localRotation;
            }

            public override bool HasChanged()
            {
                return Rotation != Target.localRotation;
            }
        }

        [SerializeField]
        Quaternion _startValue = Quaternion.identity;

        public Quaternion StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        Quaternion _endValue = Quaternion.identity;

        public Quaternion EndValue { get { return _endValue; } set { _endValue = value; } }

        public override void Copy(Step other)
        {
            base.Copy(other);

            SetOrCreateDefaultValues();

            CopyActionValues((RotationEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            CopyValues(ref _startValue, ((RotationEffect)other).StartValue);
            CopyValues(ref _endValue, ((RotationEffect)other).EndValue);
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

        static void CopyValues(ref Quaternion dest, Quaternion src)
        {
            dest = src;
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            Quaternion endRot = _endValue;
            Quaternion startRot = _startValue;

            _startValue = endRot;
            _endValue = startRot;
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

            Target.localRotation = Quaternion.LerpUnclamped(_startValue, _endValue, blend);
        }

        public override void SaveValues()
        {
            StartValue = Target.localRotation;
            EndValue = Target.localRotation;
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
                StartValue = Target.localRotation;
            }
            else
            {
                EndValue = Target.localRotation;
            }
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}
