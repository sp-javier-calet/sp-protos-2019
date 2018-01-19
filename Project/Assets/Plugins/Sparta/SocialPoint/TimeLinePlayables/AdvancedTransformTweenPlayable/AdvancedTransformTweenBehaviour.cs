using System;
using UnityEngine;
using UnityEngine.Playables;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class BaseAdvancedTweenBehaviour
    {
        public enum AnimateType
        {
            AnimationCurve,
            Tween
        }

        public enum HowToAnimateType
        {
            UseAbsoluteValues,
            UseReferencedTransforms
        }

        public bool Animate;
        public string AnimateLabel;
        public Transform ReferencedFrom;
        public Transform ReferencedTo;
        public Transform OriginalTransform;
        public bool UseCurrentFromValue;
        public bool UseCurrentToValue;
        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;
        public Transform AnimateFromReference;
        public Transform AnimateToReference;
        public HowToAnimateType HowToAnimate;
        public AnimateType AnimationType;
        public EaseType EaseType;
        public AnimationCurve AnimationCurve;

        public virtual Vector3 SetupAnimation(Transform referenceTransform, Vector3 defaultValue) 
        { 
            return Vector3.zero; 
        }
            
        public void SetupAnimationCurve(AnimationCurve defaultAnimationCurve)
        {
            if(AnimationType == AnimateType.AnimationCurve)
            {
                if(AnimationCurve.keys.Length == 0)
                {
                    AnimationCurve = defaultAnimationCurve;
                }
            }
            else
            {
                var curve = new AnimationCurve();
                AnimationCurve = curve;
            }
        }
            
//        public void InitializeValues(Transform baseTransform)
        public void InitializeValues()
        {
//            AnimateFrom = SetupAnimation(baseTransform, AnimateFromReference);
//            AnimateTo = SetupAnimation(baseTransform, AnimateToReference);

            AnimateFrom = SetupAnimation(AnimateFromReference, AnimateFrom);
            AnimateTo = SetupAnimation(AnimateToReference, AnimateTo);
        }
    }
        
    [Serializable]
    public class PositionAdvancedTweenBehaviour : BaseAdvancedTweenBehaviour
    {
        public PositionAdvancedTweenBehaviour(string labelName)
        {
            AnimateLabel = labelName;
        }

//        protected override Vector3 SetupAnimation(Transform baseTransform, Transform referenceTransform)
        public override Vector3 SetupAnimation(Transform referenceTransform, Vector3 defaultValue)
        {
            if(UseCurrentFromValue)
            {
//                return baseTransform.position;
                return referenceTransform.position;
            }
            else if(HowToAnimate == HowToAnimateType.UseReferencedTransforms)
            {
                if(referenceTransform != null)
                {
                    return referenceTransform.position;
                }
            }

            return defaultValue;
        }
    }
        
    [Serializable]
    public class RotationAdvancedTweenBehaviour : BaseAdvancedTweenBehaviour
    {
        public RotationAdvancedTweenBehaviour(string labelName)
        {
            AnimateLabel = labelName;
        }

//        protected override Vector3 SetupAnimation(Transform baseTransform, Transform referenceTransform)
        public override Vector3 SetupAnimation(Transform referenceTransform, Vector3 defaultValue)
        {
            if(UseCurrentFromValue)
            {
//                return baseTransform.rotation.eulerAngles;
                return referenceTransform.eulerAngles;
            }
            else if(HowToAnimate == HowToAnimateType.UseReferencedTransforms && referenceTransform != null)
            {
                if(referenceTransform != null)
                {
                    return referenceTransform.eulerAngles;
                }
            }

            return defaultValue;
        }
    }
        
    [Serializable]
    public class ScaleAdvancedTweenBehaviour : BaseAdvancedTweenBehaviour
    {
        public ScaleAdvancedTweenBehaviour(string labelName)
        {
            AnimateLabel = labelName;
        }

//        protected override Vector3 SetupAnimation(Transform baseTransform, Transform referenceTransform)
        public override Vector3 SetupAnimation(Transform referenceTransform, Vector3 defaultValue)
        {
            if(UseCurrentFromValue)
            {
//                return baseTransform.localScale;
                return referenceTransform.localScale;
            }
            else if(HowToAnimate == HowToAnimateType.UseReferencedTransforms && referenceTransform != null)
            {
                if(referenceTransform != null)
                {
                    return referenceTransform.localScale;
                }
            }

            return defaultValue;
        }
    }

    [Serializable]
    public class AdvancedTransformTweenBehaviour : PlayableBehaviour
    {
//        const float kRightAngleInRads = Mathf.PI * 0.5f;

        public float Duration;
        public float InverseDuration;

        public BaseAdvancedTweenBehaviour[] Animations = 
        {
            new PositionAdvancedTweenBehaviour("Animate Position"),
            new RotationAdvancedTweenBehaviour("Animate Rotation"),
            new ScaleAdvancedTweenBehaviour("Animate Scale")
        };
            
        readonly AnimationCurve defaultCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            
        public override void OnGraphStart(Playable playable)
        {
            Duration = (float)playable.GetDuration();
            if(Mathf.Approximately(Duration, 0f))
            {
                throw new UnityException("A Tween cannot have a duration of zero.");
            }
                
            InverseDuration = 1f / Duration;

            for(int i = 0; i < Animations.Length; ++i)
            {
                var anim = Animations[i];
                if(anim != null)
                {
                    anim.InitializeValues();
                    anim.SetupAnimationCurve(defaultCurve);
                }
            }
        }
    }
}