using UnityEngine;
using SocialPoint.GUIControl;
using SocialPoint.Base;

namespace SocialPoint.GUIAnimation
{
    [System.Serializable]
    public sealed class AnchorProperties
    {
        [SerializeField]
        public Vector2 AnchorMin = new Vector2(0.5f, 0.5f);
        [SerializeField]
        public Vector2 AnchorMax = new Vector2(0.5f, 0.5f);

        [SerializeField]
        public Vector2 OffsetMin = new Vector2(0.0f, 0.0f);

        [SerializeField]
        public Vector2 OffsetMax = new Vector2(0.0f, 0.0f);

        public void Copy(AnchorProperties other)
        {
            AnchorMin = other.AnchorMin;
            AnchorMax = other.AnchorMax;

            OffsetMin = other.OffsetMin;
            OffsetMax = other.OffsetMax;
        }
    }

    public interface IPositionable
    {
        AnchorProperties StartAnchor { get; set; }

        AnchorProperties EndAnchor { get; set; }

        AnchorMode AnchorsMode { get; set; }

        bool IsLocal { get; set; }

        void SetAnchors();
    }

    [System.Serializable]
    public class PositionEffect : BlendEffect, IPositionable
    {
        [System.Serializable]
        public class TargetValueMonitor : StepMonitor
        {
            public Vector3 Position;

            public override void Backup()
            {
                Position = Target.position;
            }

            public override bool HasChanged()
            {
                return Position != Target.position;
            }
        }

        [ShowInEditor]
        [SerializeField]
        AnchorProperties _startAnchor = new AnchorProperties();

        public AnchorProperties StartAnchor { get { return _startAnchor; } set { _startAnchor = value; } }

        [ShowInEditor]
        [SerializeField]
        AnchorProperties _endAnchor = new AnchorProperties();

        public AnchorProperties EndAnchor { get { return _endAnchor; } set { _endAnchor = value; } }

        [ShowInEditor]
        [SerializeField]
        AnchorMode _anchorMode = AnchorMode.Disabled;

        public AnchorMode AnchorsMode { get { return _anchorMode; } set { _anchorMode = value; } }

        [ShowInEditor]
        [SerializeField]
        bool _isLocal;

        public bool IsLocal
        {
            get
            {
                return _isLocal;
            } 
            set
            {
                if(value != _isLocal)
                {
                    if(value)
                    {
                        ChangeFromWorldToLocal();
                    }
                    else
                    {
                        ChangeFromLocalToWorld();
                    }
                }

                _isLocal = value;
            }
        }

        [SerializeField]
        Transform _startValue;

        public Transform StartValue { get { return _startValue; } set { _startValue = value; } }

        [SerializeField]
        Transform _endValue;

        public Transform EndValue { get { return _endValue; } set { _endValue = value; } }

        public override void Copy(Step other)
        {
            base.Copy(other);

            _startValue = null;
            _endValue = null;
            SetOrCreateDefaultValues();

            CopyActionValues((PositionEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            var otherTrans = (PositionEffect)other;

            _anchorMode = otherTrans.AnchorsMode;
            _startAnchor.Copy(otherTrans.StartAnchor);
            _endAnchor.Copy(otherTrans.EndAnchor);
            IsLocal = otherTrans.IsLocal;

            RemoveAnchors();
            if(otherTrans.StartValue != null && otherTrans.EndValue != null)
            {
                CopyTransformValues(_startValue, ((PositionEffect)other).StartValue);
                CopyTransformValues(_endValue, ((PositionEffect)other).EndValue);
            }

            SetAnchors();
        }

        public override void CopySharedValues(Effect other)
        {
            var otherTrans = (PositionEffect)other;

            _anchorMode = otherTrans.AnchorsMode;
            _startAnchor.Copy(otherTrans.StartAnchor);
            _endAnchor.Copy(otherTrans.EndAnchor);
            IsLocal = otherTrans.IsLocal;

            SetAnchors();
        }

        public void RemoveAnchors()
        {
            if(_startValue != null && _endValue != null)
            {
                RemoveAnchorAt(0f);
                RemoveAnchorAt(1f);
            }
        }

        public void SetAnchors()
        {
            if(_startValue != null && _endValue != null)
            {
                SetAnchorAt(0f);
                SetAnchorAt(1f);
            }
        }

        public override void SetOrCreateDefaultValues()
        {
            if(_startValue == null)
            {
                _startValue = AnchorUtility.CreatePivotTransform(StepName + "_start");
                _startValue.transform.SetParent(transform, false);
                _startValue.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            }

            if(_endValue == null)
            {
                _endValue = AnchorUtility.CreatePivotTransform(StepName + "_end");
                _endValue.transform.SetParent(transform, false);
                _endValue.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            }

            if(Target != null)
            {
                SaveValuesAt(0f);
                SaveValuesAt(1f);
            }
        }

        static void CopyTransformValues(Transform dest, Transform src)
        {
            dest.position = src.position;
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            Vector3 ensPos = _endValue.position;
            Vector3 startPos = _startValue.position;

            _startValue.position = ensPos;
            _endValue.position = startPos;

            SetAnchorAt(0f);
            SetAnchorAt(1f);
        }

        public override void OnRemoved()
        {
            if(_startValue != null)
            {
                Object.DestroyImmediate(_startValue.gameObject);
                _startValue = null;
            }
			
            if(_endValue != null)
            {
                Object.DestroyImmediate(_endValue.gameObject);
                _endValue = null;
            }
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

            SetPosition(Target, Vector3.LerpUnclamped(GetPosition(_startValue), GetPosition(_endValue), blend));
        }

        Vector3 GetPosition(Transform trans)
        {
            return IsLocal ? trans.localPosition : trans.position;
        }

        void SetPosition(Transform dest, Vector3 src)
        {
            if(IsLocal)
            {
                dest.localPosition = src;
            }
            else
            {
                dest.position = src;
            }
        }

        void SetPosition(Transform dest, Transform src)
        {
            if(IsLocal)
            {
                dest.localPosition = src.localPosition;
            }
            else
            {
                dest.position = src.position;
            }
        }

        // DeltaPosition should be world space to work properly
        public void SetMovementDelta(Vector3 deltaPostion, bool deltaInWorldSpace = true)
        {
            if(IsLocal)
            {
                if(deltaInWorldSpace)
                {
                    deltaPostion = AnchorUtility.ToPixels(deltaPostion);
                }
            }
            else
            {
                if(!deltaInWorldSpace)
                {
                    deltaPostion = AnchorUtility.ToClipSpace(deltaPostion);
                }
            }
			
            RemoveAnchors();
			
            Vector3 startPos = GetPosition(Target);
            Vector3 endPos = startPos + deltaPostion;
            SetPosition(_startValue, startPos);
            SetPosition(_endValue, endPos);
			
            SetAnchors();
        }

        public void SetMovementTo(Transform end)
        {
            RemoveAnchors();

            Vector3 startPos = GetPosition(Target);
            Vector3 endPos = GetPosition(end);

            SetPosition(_startValue, startPos);
            SetPosition(_endValue, endPos);

            SetAnchors();
        }

        public void SetTransform(float localTimeNorm, Transform trans)
        {
            RemoveAnchors();

            if(localTimeNorm < 0.5f)
            {
                CopyTransformValues(_startValue, trans);
            }
            else
            {
                CopyTransformValues(_endValue, trans);
            }

            SetAnchors();
        }

        public override void SaveValues()
        {
            RemoveAnchors();

            SetPosition(StartValue, Target);
            SetPosition(EndValue, Target);

            SetAnchors();
        }

        public override void SaveValuesAt(float localTimeNormalized)
        {
            if(Target == null)
            {
                if(Animation != null && Animation.EnableWarnings)
                {
                    Log.w("[TransformEffect] Target is null");
                }
                return;
            }

            if(localTimeNormalized < 0.5f)
            {
                RemoveAnchorAt(0f);

                SetPosition(StartValue, Target);

                SetAnchorAt(0f);
            }
            else
            {
                RemoveAnchorAt(1f);

                SetPosition(EndValue, Target);

                SetAnchorAt(1f);
            }
        }

        void RemoveAnchorAt(float localTime)
        {
            Transform trans = localTime < 0.5f ? _startValue : _endValue;
            AnchorUtility.RemoveAnchors(trans, false);
            IGraphicObject graphic = GraphicObjectLoader.Load(trans, false);
            if(graphic != null)
            {
                graphic.Refresh();
            }
        }

        void SetAnchorAt(float localTime)
        {
            Transform trans = localTime < 0.5f ? _startValue : _endValue;
            AnchorProperties anchorsValue = localTime < 0.5f ? _startAnchor : _endAnchor;

            Transform parent = null;
            if(Animation != null)
            {
                UIViewController view = Animation.GetComponentInParent<UIViewController>();
                parent = view != null ? view.transform : null;
            }

            if(_anchorMode == AnchorMode.Disabled)
            {
                AnchorUtility.RemoveAnchors(trans, false);
            }
            else if(_anchorMode == AnchorMode.Custom)
            {
                AnchorUtility.SetAnchors(trans, parent, anchorsValue.AnchorMin, anchorsValue.AnchorMax, false);
            }
            else
            {
                Vector2[] anchors = AnchorUtility.SetAnchors(trans, parent, AnchorsMode, false);
                anchorsValue.AnchorMin = anchors[0];
                anchorsValue.AnchorMax = anchors[1];
            }

            IGraphicObject graphic = GraphicObjectLoader.Load(trans, false);
            if(graphic != null)
            {
                graphic.Refresh();
            }
        }

        void DoUpdateAnchors()
        {
            AnchorUtility.Update(Target, true);

            AnchorUtility.Update(StartValue, true);
            AnchorUtility.Update(EndValue, true);
        }

        void DoRemoveTargetAnchors()
        {
            AnchorUtility.RemoveAnchors(Target, true);
        }

        void ChangeFromWorldToLocal()
        {
            if(Target == null)
            {
                return;
            }

            ChangeFromWorldToLocalTransform(StartValue);
            ChangeFromWorldToLocalTransform(EndValue);
        }

        void ChangeFromWorldToLocalTransform(Transform pivot)
        {
            Vector3 currPos = Target.position;

            Target.position = pivot.position;
            pivot.localPosition = Target.localPosition;

            Target.position = currPos;
        }

        void ChangeFromLocalToWorld()
        {
            if(Target == null)
            {
                return;
            }

            ChangeFromLocalToWorldTransform(StartValue);
            ChangeFromLocalToWorldTransform(EndValue);
        }

        void ChangeFromLocalToWorldTransform(Transform pivot)
        {
            Vector3 currPos = Target.position;

            Target.localPosition = pivot.localPosition;
            pivot.position = Target.position;

            Target.position = currPos;
        }

        public override StepMonitor CreateTargetMonitor()
        {
            return new TargetValueMonitor();
        }
    }
}