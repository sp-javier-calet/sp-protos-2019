#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject
{
    [TaskCategory("Basic/GameObject")]
    [TaskDescription("Finds a GameObject by tag. Returns Success.")]
    public class FindGameObjectsWithTag : Action
    {
        [Tooltip("The tag of the GameObject to find")]
        public SharedString tag;
        [Tooltip("The objects found by name")]
        [RequiredField]
        public SharedGameObjectList storeValue;

        public override TaskStatus OnUpdate()
        {
            var gameObjects = GameObject.FindGameObjectsWithTag(tag.Value);
            for (int i = 0; i < gameObjects.Length; ++i) {
                storeValue.Value.Add(gameObjects[i]);
            }

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            tag.Value = null;
            storeValue.Value = null;
        }
    }
}
#endif