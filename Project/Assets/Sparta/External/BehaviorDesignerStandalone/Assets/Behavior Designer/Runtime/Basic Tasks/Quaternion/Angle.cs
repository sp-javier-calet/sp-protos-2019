#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityQuaternion
{
    [TaskCategory("Basic/Quaternion")]
    [TaskDescription("Stores the angle in degrees between two rotations.")]
    public class Angle : Action
    {
        [Tooltip("The first rotation")]
        public SharedQuaternion firstRotation;
        [Tooltip("The second rotation")]
        public SharedQuaternion secondRotation;
        [Tooltip("The stored result")]
        [RequiredField]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = Quaternion.Angle(firstRotation.Value, secondRotation.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            firstRotation = secondRotation = Quaternion.identity;
            storeResult = 0;
        }
    }
}
#endif