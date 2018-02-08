using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [Serializable, NotKeyable]
    public class DestroyPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public DestroyPlayableData Template = new DestroyPlayableData();
        public ExposedReference<GameObject> GameObject;
        public TimelineClip CustomClipReference { get; set; }

        public ClipCaps clipCaps
        {
            get { return ClipCaps.None; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DestroyPlayableData>.Create(graph, Template);
            var clone = playable.GetBehaviour();
            clone.GameObject = GameObject.Resolve(graph.GetResolver());

            if(clone.GameObject != null)
            {
                clone.InitialActiveState = clone.GameObject.activeSelf;
            }

            clone.CustomClipReference = CustomClipReference;

            return playable;
        }
    }
}