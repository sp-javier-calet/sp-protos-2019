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
        public ExposedReference<Transform> TransformFrom;
        public ExposedReference<Transform> TransformTo;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ScaleTweenPlayableBehaviour>.Create(graph, template);

            var clone = playable.GetBehaviour();
            clone.TransformFrom = TransformFrom.Resolve(graph.GetResolver());
            clone.TransformTo = TransformTo.Resolve(graph.GetResolver());

            return playable;
        }
    }
}
