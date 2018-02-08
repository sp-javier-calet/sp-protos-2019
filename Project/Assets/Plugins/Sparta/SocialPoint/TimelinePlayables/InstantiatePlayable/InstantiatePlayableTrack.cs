using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [TrackColor(0.5f, 0.5f, 0.5f)]
    [TrackClipType(typeof(InstantiatePlayableAsset))]
    public class InstantiatePlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<InstantiatePlayableMixer>.Create(graph, inputCount);

            var iter = GetClips().GetEnumerator();
            while(iter.MoveNext())
            {
                var clip = iter.Current;
                if(clip != null)
                {
                    var customClip = clip.asset as InstantiatePlayableAsset;
                    if(customClip != null)
                    {
                        customClip.CustomClipReference = clip;
                    }
                }
            }
            iter.Dispose();

            return mixer;
        }
    }
}
