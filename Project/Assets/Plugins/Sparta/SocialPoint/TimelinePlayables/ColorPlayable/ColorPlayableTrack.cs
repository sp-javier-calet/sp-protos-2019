using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace SocialPoint.TimelinePlayables
{
    [TrackColor(1f, 0.5f, 0.5f)]
    [TrackClipType(typeof(ColorPlayableAsset))]
    [TrackBindingType(typeof(MaskableGraphic))]
    public class ColorPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<ColorPlayableMixer>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var trackBinding = director.GetGenericBinding(this) as MaskableGraphic;
            if(trackBinding == null)
            {
                return;
            }

            driver.AddFromName<MaskableGraphic>(trackBinding.gameObject, "m_Color");
#endif

            base.GatherProperties(director, driver);
        }
    }
}
