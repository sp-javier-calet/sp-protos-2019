using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class ColorPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public ColorPlayableData Template = new ColorPlayableData();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ColorPlayableData>.Create(graph, Template);
        }
    }
}