using System;
using UnityEngine;
using UnityEngine.Playables;
using SocialPoint.Utils;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class AdvancedTweenBehaviour
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

        public void Initialize(int index)
        {
//            if(UseCurrentFromValue)
//            {
//                switch(index)
//                {
//                case AdvancedTransformTweenBehaviour.kAnimatePosition:
//                    AnimateFrom = AnimateFromReference.position;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateRotation:
//                    AnimateFrom = AnimateFromReference.rotation.eulerAngles;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateScale:
//                    AnimateFrom = AnimateFromReference.localScale;
//                    break;
//                }
//            }
//            else if(AnimateFromReference != null && HowToAnimate == HowToAnimateType.UseReferencedTransforms)
//            {
//                switch(index)
//                {
//                case AdvancedTransformTweenBehaviour.kAnimatePosition:
//                    AnimateFrom = AnimateFromReference.position;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateRotation:
//                    AnimateFrom = AnimateFromReference.rotation.eulerAngles;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateScale:
//                    AnimateFrom = AnimateFromReference.localScale;
//                    break;
//                }
//            }
//
//            if(UseCurrentToValue || (AnimateToReference != null && HowToAnimate == HowToAnimateType.UseReferencedTransforms))
//            {
//                switch(index)
//                {
//                case AdvancedTransformTweenBehaviour.kAnimatePosition:
//                    AnimateTo = AnimateFromReference.position;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateRotation:
//                    AnimateTo = AnimateFromReference.rotation.eulerAngles;
//                    break;
//
//                case AdvancedTransformTweenBehaviour.kAnimateScale:
//                    AnimateTo = AnimateFromReference.localScale;
//                    break;
//                }
//            }

            if(AnimationType == AnimateType.AnimationCurve)
            {
                if(AnimationCurve.keys.Length == 0)
                {
                    // Create a base linear curve to be the default curve to be used
                    var curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
                    AnimationCurve = curve;
                }
            }
            else
            {
                var curve = new AnimationCurve();
                AnimationCurve = curve;
            }
        }
    }

    [Serializable]
    public class AdvancedTransformTweenBehaviour : PlayableBehaviour
    {
        const float kRightAngleInRads = Mathf.PI * 0.5f;

        public bool AnimatePosition;
        public bool AnimateRotation;
        public bool AnimateScale;
        public float Duration;
        public float InverseDuration;

        public const int kAnimatePosition = 0;
        public const int kAnimateRotation = 1;
        public const int kAnimateScale = 2;
        public const int kAnimateTotal = 3;

        public AdvancedTweenBehaviour[] Animations = 
        {
            new AdvancedTweenBehaviour(),
            new AdvancedTweenBehaviour(),
            new AdvancedTweenBehaviour()
        };

        public override void OnGraphStart(Playable playable)
        {
            Duration = (float)playable.GetDuration();
            if(Mathf.Approximately(Duration, 0f))
            {
                throw new UnityException("A Tween cannot have a duration of zero.");
            }
                
            InverseDuration = 1f / Duration;

            if(AnimatePosition)
            {
                var anim = Animations[kAnimatePosition];
                if(anim != null)
                {
                    anim.Initialize(kAnimatePosition);
                }
            }

            if(AnimateRotation)
            {
                var anim = Animations[kAnimateRotation];
                if(anim != null)
                {
                    anim.Initialize(kAnimateRotation);
                }
            }

            if(AnimateScale)
            {
                var anim = Animations[kAnimateScale];
                if(anim != null)
                {
                    anim.Initialize(kAnimateScale);
                }
            }
        }
    }
}