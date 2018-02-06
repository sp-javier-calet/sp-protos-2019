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

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<OpacityPlayableData>.Create(graph, Template);
        }
    }
}