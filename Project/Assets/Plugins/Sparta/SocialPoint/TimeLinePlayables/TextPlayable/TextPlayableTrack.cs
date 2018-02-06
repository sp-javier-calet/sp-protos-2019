using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using SocialPoint.GUIControl;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(0f, 0.5f, 1f)]
    [TrackClipType(typeof(TextPlayableAsset))]
    [TrackBindingType(typeof(SPText))]
    public class TextPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<TextPlayableMixer>.Create(graph, inputCount);

            var iter = GetClips().GetEnumerator();
            while(iter.MoveNext())
            {
                var clip = iter.Current;
                if(clip != null)
                {
                    var customClip = clip.asset as TextPlayableAsset;
                    if(customClip != null)
                    {
                        customClip.CustomClipReference = clip;
                    }
                }
            }
            iter.Dispose();

            return mixer;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var trackBinding = director.GetGenericBinding(this) as SPText;
            if(trackBinding == null)
            {
                return;
            }

            var serializedObject = new UnityEditor.SerializedObject(trackBinding);
            var iterator = serializedObject.GetIterator();
            while(iterator.NextVisible(true))
            {
                if(iterator.hasVisibleChildren)
                {
                    continue;
                }

                driver.AddFromName<SPText>(trackBinding.gameObject, iterator.propertyPath);
            }
#endif

            base.GatherProperties(director, driver);
        }
    }
}
