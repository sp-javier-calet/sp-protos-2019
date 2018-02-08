using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimelinePlayables
{
    [TrackColor(0f, 1f, 1f)]
    [TrackClipType(typeof(RotationPlayableAsset))]
    [TrackBindingType(typeof(Transform))]
    public class RotationPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<RotationPlayableMixer>.Create(graph, inputCount);

            var iter = GetClips().GetEnumerator();
            while(iter.MoveNext())
            {
                var clip = iter.Current;
                if(clip != null)
                {
                    var customClip = clip.asset as RotationPlayableAsset;
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
            var trackBinding = director.GetGenericBinding(this) as Transform;
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

                driver.AddFromName<Transform>(trackBinding.gameObject, iterator.propertyPath);
            }
            #endif

            base.GatherProperties(director, driver);
        }
    }
}
