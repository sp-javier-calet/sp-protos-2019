#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityGameObject
{
    [TaskCategory("Basic/GameObject")]
    [TaskDescription("Sets the GameObject tag. Returns Success.")]
    public class SetTag : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [Tooltip("The GameObject tag")]
        public SharedString tag;

        public override TaskStatus OnUpdate()
        {
            GetDefaultGameObject(targetGameObject.Value).tag = tag.Value;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            tag = "";
        }
    }
}
#endif