﻿using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [Serializable]
    public class RotationTweenPlayableClip : PlayableAsset, ITimelineClipAsset
    {
        public RotationTweenPlayableBehaviour Template = new RotationTweenPlayableBehaviour();
        public ExposedReference<Transform> TransformFrom;
        public ExposedReference<Transform> TransformTo;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<RotationTweenPlayableBehaviour>.Create(graph, Template);

            var clone = playable.GetBehaviour();
            clone.TransformFrom = TransformFrom.Resolve(graph.GetResolver());
            clone.TransformTo = TransformTo.Resolve(graph.GetResolver());

            return playable;
        }
    }
}