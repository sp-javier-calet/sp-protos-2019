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
            return ScriptPlayable<OpacityPlayableMixer>.Create(graph, inputCount);
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
