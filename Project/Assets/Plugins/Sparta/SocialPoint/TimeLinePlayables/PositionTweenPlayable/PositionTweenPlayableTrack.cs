using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace SocialPoint.TimeLinePlayables
{
    [TrackColor(0f, 0f, 1f)]
    [TrackClipType(typeof(PositionTweenPlayableClip))]
    [TrackBindingType(typeof(Transform))]
    public class PositionTweenPlayableTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<PositionTweenPlayableMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
#if UNITY_EDITOR
            var trackBinding = director.GetGenericBinding(this) as Transform;
            if(trackBinding == null)
            {
                return;
            }

            var defaultPosition = trackBinding.position;
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
