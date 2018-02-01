using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class TimeScalePlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public TimeScalePlayableData Template = new TimeScalePlayableData();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Extrapolation | ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<TimeScalePlayableData>.Create(graph, Template);
        }
    }
}
