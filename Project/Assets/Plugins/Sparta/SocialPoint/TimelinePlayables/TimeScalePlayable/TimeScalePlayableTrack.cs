using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [TrackColor(0.5f, 0f, 0f)]
    [TrackClipType(typeof(TimeScalePlayableAsset))]
    public class TimeScalePlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TimeScalePlayableMixer>.Create(graph, inputCount);
        }
    }
}
