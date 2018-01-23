using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(0.5f, 0f, 0f)]
    [TrackClipType(typeof(TimeScalePlayableClip))]
    public class TimeScalePlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TimeScalePlayableMixerBehaviour>.Create(graph, inputCount);
        }
    }
}
