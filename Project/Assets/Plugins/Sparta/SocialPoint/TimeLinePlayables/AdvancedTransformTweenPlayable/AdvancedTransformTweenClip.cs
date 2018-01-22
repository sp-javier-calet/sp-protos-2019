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
        public ExposedReference<Transform> PositionReferencedFrom;
        public ExposedReference<Transform> PositionReferencedTo;

        public ExposedReference<Transform> RotationReferencedFrom;
        public ExposedReference<Transform> RotationReferencedTo;

        public ExposedReference<Transform> ScaleReferencedFrom;
        public ExposedReference<Transform> ScaleReferencedTo;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.Blending; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AdvancedTransformTweenBehaviour>.Create(graph, template);

//            var clone = playable.GetBehaviour();
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimatePosition].AnimateFromReference = PositionReferencedFrom.Resolve(graph.GetResolver());
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimatePosition].AnimateFromReference = PositionReferencedTo.Resolve(graph.GetResolver());
//
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimateRotation].AnimateFromReference = RotationReferencedFrom.Resolve(graph.GetResolver());
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimateRotation].AnimateFromReference = RotationReferencedTo.Resolve(graph.GetResolver());
//
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimateScale].AnimateFromReference = ScaleReferencedFrom.Resolve(graph.GetResolver());
//            clone.Animations[AdvancedTransformTweenBehaviour.kAnimateScale].AnimateFromReference = ScaleReferencedTo.Resolve(graph.GetResolver());

            return playable;
        }
    }
}