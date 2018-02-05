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
            var mixer = ScriptPlayable<DestroyPlayableMixer>.Create(graph, inputCount);

            var iter = GetClips().GetEnumerator();
            while(iter.MoveNext())
            {
                var clip = iter.Current;
                if(clip != null)
                {
                    var customClip = clip.asset as DestroyPlayableAsset;
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
