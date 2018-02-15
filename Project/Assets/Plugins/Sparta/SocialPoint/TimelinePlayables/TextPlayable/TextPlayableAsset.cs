using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class TextPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public TextPlayableData Template = new TextPlayableData();
        public TimelineClip CustomClipReference { get; set; }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TextPlayableData>.Create(graph, Template);
            var clone = playable.GetBehaviour();
            clone.CustomClipReference = CustomClipReference;

            return playable;
        }
    }
}
