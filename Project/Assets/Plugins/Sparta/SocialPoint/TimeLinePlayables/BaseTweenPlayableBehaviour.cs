using System;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class BaseTweenPlayableBehaviour : PlayableBehaviour
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
        public float Duration;
        public float InverseDuration;
        public Transform TransformFrom;
        public Transform TransformTo;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            var duration = playable.GetDuration();
            if(Mathf.Approximately((float)duration, 0f))
            {
                throw new UnityException("A Tween cannot have a duration of zero.");
            }

            Duration = (float)duration;
            InverseDuration = 1f / (float)duration;
        }
    }
}
