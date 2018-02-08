using System;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class BaseTransformPlayableData : BasePlayableData
    {
        readonly static AnimationCurve defaultAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public enum HowToAnimateType
        {
            UseInitialTransformValues,
            UseAbsoluteValues,
            UseReferenceTransform
        }

        public enum AnimateType
        {
            AnimationCurve,
            Tween
        }
            
        public HowToAnimateType HowToAnimateFrom = HowToAnimateType.UseInitialTransformValues;
        public HowToAnimateType HowToAnimateTo = HowToAnimateType.UseAbsoluteValues;
        public AnimateType AnimationType = AnimateType.AnimationCurve;
        public EaseType EaseType = EaseType.Linear;
        public AnimationCurve AnimationCurve = defaultAnimationCurve;
        public Transform TransformFrom;
        public Transform TransformTo;
    }
}
