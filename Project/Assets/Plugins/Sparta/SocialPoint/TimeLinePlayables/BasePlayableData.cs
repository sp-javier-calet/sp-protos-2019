using System;
using UnityEngine;
using UnityEngine.Playables;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class BasePlayableData : PlayableBehaviour
    {
        public float Duration;
        public float InverseDuration;

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
