using SocialPoint.Base;
using SocialPoint.GUIControl;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // Deprecated
    [System.Serializable]
    public sealed class TransformEffect : BlendEffect, IPositionable
    {
        [System.Serializable]
        public class TargetValueMonitor : StepMonitor
        {
            public Vector3 Position;
            public Vector3 Scale;
            public Quaternion Rotation;

            public override void Backup()
            {
                Position = Target.position;
                Scale = Target.localScale;
                Rotation = Target.rotation;
            }

            public override bool HasChanged()
            {
                return Position != Target.position
                || Scale != Target.localScale
                || Rotation != Target.rotation;
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

        [ShowInEditor]
        [SerializeField]
        bool _freezePosition;

        public bool FreezePosition { get { return _freezePosition; } set { _freezePosition = value; } }

        [ShowInEditor]
        [SerializeField]
        bool _freezeRotation;

        public bool FreezeRotation { get { return _freezeRotation; } set { _freezeRotation = value; } }

        [ShowInEditor]
        [SerializeField]
        bool _freezeScale;

        public bool FreezeScale { get { return _freezeScale; } set { _freezeScale = value; } }

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

            CopyActionValues((TransformEffect)other);
        }

        public override void CopyActionValues(Effect other)
        {
            var otherTrans = (TransformEffect)other;

            _anchorMode = otherTrans.AnchorsMode;
            _startAnchor.Copy(otherTrans.StartAnchor);
            _endAnchor.Copy(otherTrans.EndAnchor);
            IsLocal = otherTrans.IsLocal;
            _freezePosition = otherTrans.FreezePosition;
            _freezeScale = otherTrans.FreezeScale;
            _freezeRotation = otherTrans.FreezeRotation;

            RemoveAnchors();
            if(otherTrans.StartValue != null && otherTrans.EndValue != null)
            {
                CopyTransformValues(_startValue, ((TransformEffect)other).StartValue);
                CopyTransformValues(_endValue, ((TransformEffect)other).EndValue);
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
                _startValue.gameObject.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
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
            dest.localScale = src.localScale;
            dest.rotation = src.rotation;
        }

        public override void Invert(bool invertTime = false)
        {
            base.Invert(invertTime);

            Vector3 ensPos = _endValue.position;
            Vector3 endScale = _endValue.localScale;
            Quaternion endRot = _endValue.rotation;

            Vector3 startPos = _startValue.position;
            Vector3 startScale = _startValue.localScale;
            Quaternion startRot = _startValue.rotation;

            _startValue.position = ensPos;
            _startValue.localScale = endScale;
            _startValue.rotation = endRot;

            _endValue.position = startPos;
            _endValue.localScale = startScale;
            _endValue.rotation = startRot;

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

            if(!FreezePosition)
            {
                SetPosition(Target, Vector3.LerpUnclamped(GetPosition(_startValue), GetPosition(_endValue), blend));
            }
            if(!FreezeScale)
            {
                Target.localScale = Vector3.LerpUnclamped(_startValue.localScale, _endValue.localScale, blend);
            }
            if(!FreezeRotation)
            {
                Target.rotation = Quaternion.SlerpUnclamped(_startValue.rotation, _endValue.rotation, blend);
            }
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
            StartValue.localScale = Target.localScale;
            StartValue.rotation = Target.rotation;

            SetPosition(EndValue, Target);
            EndValue.localScale = Target.localScale;
            EndValue.rotation = Target.rotation;

            SetAnchors();
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
                RemoveAnchorAt(0f);

                SetPosition(StartValue, Target);
                StartValue.localScale = Target.localScale;
                StartValue.rotation = Target.rotation;

                SetAnchorAt(0f);
            }
            else
            {
                RemoveAnchorAt(1f);

                SetPosition(EndValue, Target);
                EndValue.localScale = Target.localScale;
                EndValue.rotation = Target.rotation;

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
