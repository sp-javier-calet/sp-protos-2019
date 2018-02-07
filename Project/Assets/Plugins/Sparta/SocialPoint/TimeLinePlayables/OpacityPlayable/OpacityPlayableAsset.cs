using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class OpacityPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public OpacityPlayableData Template = new OpacityPlayableData();
        public TimelineClip CustomClipReference { get; set; }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<OpacityPlayableData>.Create(graph, Template);
            var clone = playable.GetBehaviour();
            clone.CustomClipReference = CustomClipReference;

            return playable;
        }
    }
}