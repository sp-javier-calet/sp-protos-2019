using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(0.5f, 0.5f, 0.5f)]
    [TrackClipType(typeof(DestroyPlayableAsset))]
    public class DestroyPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<DestroyPlayableMixer>.Create(graph, inputCount);
        }
    }
}
