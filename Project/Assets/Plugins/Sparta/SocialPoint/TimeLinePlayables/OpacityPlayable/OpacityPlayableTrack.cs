using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(1f, 0.5f, 0.5f)]
    [TrackClipType(typeof(OpacityPlayableAsset))]
    [TrackBindingType(typeof(CanvasGroup))]
    public class OpacityPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<OpacityPlayableMixer>.Create(graph, inputCount);

            var iter = GetClips().GetEnumerator();
            while(iter.MoveNext())
            {
                var clip = iter.Current;
                if(clip != null)
                {
                    var customClip = clip.asset as OpacityPlayableAsset;
                    if(customClip != null)
                    {
                        customClip.CustomClipReference = clip;
                    }
                }
            }
            iter.Dispose();

            return mixer;
        }

//        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
//        {
//#if UNITY_EDITOR
//            var trackBinding = director.GetGenericBinding(this) as CanvasGroup;
//            if(trackBinding == null)
//            {
//                return;
//            }

//            driver.AddFromName<CanvasGroup>(trackBinding.gameObject, "alpha");
//#endif

        //    base.GatherProperties(director, driver);
        //}
    }
}
