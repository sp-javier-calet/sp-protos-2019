#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif
namespace BehaviorDesigner.Runtime.Tasks.Basic.SharedVariables
{
    [TaskCategory("Basic/SharedVariable")]
    [TaskDescription("Sets the SharedQuaternion variable to the specified object. Returns Success.")]
    public class SetSharedQuaternion : Action
    {
        [Tooltip("The value to set the SharedQuaternion to")]
        public SharedQuaternion targetValue;
        [RequiredField]
        [Tooltip("The SharedQuaternion to set")]
        public SharedQuaternion targetVariable;

        public override TaskStatus OnUpdate()
        {
            targetVariable.Value = targetValue.Value;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetValue = Quaternion.identity;
            targetVariable = Quaternion.identity;
        }
    }
}