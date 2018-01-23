using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class TimeScalePlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public TimeScalePlayableBehaviour Template = new TimeScalePlayableBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Extrapolation | ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TimeScalePlayableBehaviour>.Create(graph, Template);
            return playable;
        }
    }
}
