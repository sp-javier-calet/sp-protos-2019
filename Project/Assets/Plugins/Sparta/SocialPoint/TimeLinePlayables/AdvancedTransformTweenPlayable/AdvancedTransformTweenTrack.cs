using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(0f, 0f, 1f)]
    [TrackClipType(typeof(AdvancedTransformTweenClip))]
    [TrackBindingType(typeof(Transform))]
    public class AdvancedTransformTweenTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<AdvancedTransformTweenMixerBehaviour>.Create(graph, inputCount);
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