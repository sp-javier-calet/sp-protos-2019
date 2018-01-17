using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class PositionTweenPlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public PositionTweenPlayableBehaviour template = new PositionTweenPlayableBehaviour();
        public ExposedReference<Transform> StartLocation;
        public ExposedReference<Transform> EndLocation;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<PositionTweenPlayableBehaviour>.Create(graph, template);

            var clone = playable.GetBehaviour();
            clone.StartLocation = StartLocation.Resolve(graph.GetResolver());
            clone.EndLocation = EndLocation.Resolve(graph.GetResolver());

            return playable;
        }
    }
}