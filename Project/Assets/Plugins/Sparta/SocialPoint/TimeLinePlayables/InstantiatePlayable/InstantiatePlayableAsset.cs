using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class InstantiatePlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public InstantiatePlayableData Template = new InstantiatePlayableData();
        public ExposedReference<GameObject> Prefab;
        public ExposedReference<Transform> Parent;
        public TimelineClip CustomClipReference { get; set; }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<InstantiatePlayableData>.Create(graph, Template);
            var clone = playable.GetBehaviour();
            clone.Prefab = Prefab.Resolve(graph.GetResolver());
            clone.Parent = Parent.Resolve(graph.GetResolver());
            clone.CustomClipReference = CustomClipReference;

            return playable;
        }
    }
}