using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable, NotKeyable]
    public class PositionPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public PositionPlayableData Template = new PositionPlayableData();
        public ExposedReference<Transform> TransformFrom;
        public ExposedReference<Transform> TransformTo;
        public TimelineClip CustomClipReference { get; set; }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PositionPlayableData>.Create(graph, Template);
            var clone = playable.GetBehaviour();
            clone.TransformFrom = TransformFrom.Resolve(graph.GetResolver());
            clone.TransformTo = TransformTo.Resolve(graph.GetResolver());
            clone.CustomClipReference = CustomClipReference;

            return playable;
        }
    }
}