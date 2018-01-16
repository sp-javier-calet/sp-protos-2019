using System;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class ScaleTweenPlayableBehaviour : PlayableBehaviour
    {
        public enum TweeningType
        {
            AnimationCurve,
            Tween
        }

        public Vector3 AnimateFrom;
        public Vector3 AnimateTo;
        public TweeningType AnimationType;
        public EaseType EaseType;
        public AnimationCurve AnimationCurve;
        public float Duration;
        public float InverseDuration;

        public override void OnGraphStart(Playable playable)
        {
            var duration = playable.GetDuration();
            if(Mathf.Approximately((float)duration, 0f))
            {
                throw new UnityException("A Tween cannot have a duration of zero.");
            }

            Duration = (float)duration;
            InverseDuration = 1f / (float)duration;

            if(AnimationType == TweeningType.AnimationCurve)
            {
                if(AnimationCurve == null)
                {
                    AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                }
            }
            else
            {
                AnimationCurve = null;
            }
        }
    }
}
