using System;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class TimeScalePlayableData : BasePlayableData
    {
        public float TimeScale = 1f;
    }
}
