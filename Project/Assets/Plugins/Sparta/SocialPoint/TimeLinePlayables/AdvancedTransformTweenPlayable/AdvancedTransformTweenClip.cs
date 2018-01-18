using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class AdvancedTransformTweenClip : PlayableAsset, ITimelineClipAsset
    {
        public AdvancedTransformTweenBehaviour template = new AdvancedTransformTweenBehaviour();
//        public ExposedReference<Transform> StartLocation;
//        public ExposedReference<Transform> EndLocation;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AdvancedTransformTweenBehaviour>.Create(graph, template);

//            var clone = playable.GetBehaviour();
//            clone.StartLocation = StartLocation.Resolve(graph.GetResolver());
//            clone.EndLocation = EndLocation.Resolve(graph.GetResolver());

            return playable;
        }
    }
}