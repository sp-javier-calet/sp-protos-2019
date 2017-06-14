#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityQuaternion
{
    [TaskCategory("Basic/Quaternion")]
    [TaskDescription("Stores a rotation which rotates from the first direction to the second.")]
    public class FromToRotation : Action
    {
        [Tooltip("The from rotation")]
        public SharedVector3 fromDirection;
        [Tooltip("The to rotation")]
        public SharedVector3 toDirection;
        [Tooltip("The stored result")]
        [RequiredField]
        public SharedQuaternion storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = Quaternion.FromToRotation(fromDirection.Value, toDirection.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            fromDirection = toDirection = Vector3.zero;
            storeResult = Quaternion.identity;
        }
    }
}
#endif