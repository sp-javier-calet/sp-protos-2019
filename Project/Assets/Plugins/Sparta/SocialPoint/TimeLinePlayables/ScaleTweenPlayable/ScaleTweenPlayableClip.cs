using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class ScaleTweenPlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public ScaleTweenPlayableBehaviour template = new ScaleTweenPlayableBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ScaleTweenPlayableBehaviour>.Create(graph, template);
        }
    }
}
