using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class AnchorEffectProps
    {
        [SerializeField]
        public Vector2 AnchorMin = new Vector2(0.0f, 0.5f);
        [SerializeField]
        public Vector2 AnchorMax = new Vector2(1.0f, 0.5f);

        [SerializeField]
        public Vector2 OffsetMin = new Vector2(0.0f, 0.0f);

        [SerializeField]
        public Vector2 OffsetMax = new Vector2(0.0f, 0.0f);

        public void Copy(AnchorEffectProps other)
        {
            AnchorMin = other.AnchorMin;
            AnchorMax = other.AnchorMax;

            OffsetMin = other.OffsetMin;
            OffsetMax = other.OffsetMax;
        }

        public void Save(Transform trans)
        {
            AnchorUtility.GetAnchors(trans, out AnchorMin, out AnchorMax, out OffsetMin, out OffsetMax);
        }
    }

    [System.Serializable]
    public sealed class AnchorsEffect : BlendEffect
    {
        [System.Serializable]
        public sealed class TargetValueMonitor : StepMonitor
        {
            public Vector2 AnchorsMin;
            public Vector2 AnchorsMax;

            public Vector2 OffsetsMin;
            public Vector2 OffsetsMax;

            public override void Backup()
            {
                AnchorUtility.GetAnchors(Target, out AnchorsMin, out AnchorsMax, out OffsetsMin, out OffsetsMax);
            }

            public override bool HasChanged()
            {
                Vector2 AnchorsMinTemp;
                Vector2 AnchorsMaxTemp;
				
                Vector2 OffsetsMinTemp;
                Vector2 OffsetsMaxTemp;

                if(AnchorUtility.GetAnchors(Target, out AnchorsMinTemp, out AnchorsMaxTemp, out OffsetsMinTemp, out OffsetsMaxTemp))
                {
                    return AnchorsMin != AnchorsMinTemp
                    || AnchorsMax != AnchorsMaxTemp
							
                    || OffsetsMin != OffsetsMinTemp
                    || OffsetsMax != OffsetsMaxTemp;
                }

                return false;
            }
        }

        [ShowInEditor]
        [SerializeField]
        AnchorEffectProps _startValue = new AnchorEffectProps();

        public AnchorEffectProps StartValue { get { return _startValue; } set { _startValue = value; } }

        [ShowInEditor]
        [SerializeField]
        AnchorEffectProps _endValue = new AnchorEffectProps();

        public AnchorEffectProps EndValue { get { return _endValue; } set { _endValue = value; } }

        public override void Copy(Step other)
        {
            base.Copy(other);

            SetOrCreateDefaultValues();
            CopyActionValues((AnchorsEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            var otherTrans = (AnchorsEffect)other;

            _startValue.Copy(otherTrans.StartValue);
            _endValue.Copy(otherTrans.EndValue);
        }

        public override void SetOrCreateDefaultValues()
        {
            if(Target != null)
            {
                SaveValuesAt(0f);
                SaveValuesAt(1f);
            }
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            AnchorEffectProps temp = _endValue;

            _endValue = _startValue;
            _startValue = temp;
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
                    Log.w("(SPTransform) OnBlend " + StepName + " Target is null");
                }
                return;
            }

            // Lerp Anchors
            Vector2 anchorsMin;
            anchorsMin.x = Mathf.LerpUnclamped(_startValue.AnchorMin.x, _endValue.AnchorMin.x, blend);
            anchorsMin.y = Mathf.LerpUnclamped(_startValue.AnchorMin.y, _endValue.AnchorMin.y, blend);

            Vector2 anchorsMax;
            anchorsMax.x = Mathf.LerpUnclamped(_startValue.AnchorMax.x, _endValue.AnchorMax.x, blend);
            anchorsMax.y = Mathf.LerpUnclamped(_startValue.AnchorMax.y, _endValue.AnchorMax.y, blend);

            // Lerp Offsets
            Vector2 offsetsMin;
            offsetsMin.x = Mathf.LerpUnclamped(_startValue.OffsetMin.x, _endValue.OffsetMin.x, blend);
            offsetsMin.y = Mathf.LerpUnclamped(_startValue.OffsetMin.y, _endValue.OffsetMin.y, blend);

            Vector2 offsetsMax;
            offsetsMax.x = Mathf.LerpUnclamped(_startValue.OffsetMax.x, _endValue.OffsetMax.x, blend);
            offsetsMax.y = Mathf.LerpUnclamped(_startValue.OffsetMax.y, _endValue.OffsetMax.y, blend);

            AnchorUtility.SetAnchors(Target, anchorsMin, anchorsMax, offsetsMin, offsetsMax);
            AnchorUtility.Update(Target, false);
        }

        public void SetCurrentPosition()
        {
            Transform parent = AnchorUtility.GetAnchorParent(Target);
            AnchorUtility.SetAnchors(Target, parent ?? Target.parent, AnchorMode.CurrentPosition, false);
        }

        public override void SaveValues()
        {
            StartValue.Save(Target);
            EndValue.Save(Target);
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
                StartValue.Save(Target);
            }
            else
            {
                EndValue.Save(Target);
            }
        }

        void DoRemoveTargetAnchors()
        {
            AnchorUtility.RemoveAnchors(Target, true);
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}