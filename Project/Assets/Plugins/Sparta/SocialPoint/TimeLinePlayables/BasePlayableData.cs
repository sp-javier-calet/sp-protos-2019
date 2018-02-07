using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class BasePlayableData : PlayableBehaviour
    {
        public TimelineClip CustomClipReference { get; set; }
        public double CustomClipStart
        {
            get
            {
                return CustomClipReference.start;
            }
        }

        public double CustomClipEnd
        {
            get
            {
                return CustomClipReference.end;
            }
        }
    }
}
