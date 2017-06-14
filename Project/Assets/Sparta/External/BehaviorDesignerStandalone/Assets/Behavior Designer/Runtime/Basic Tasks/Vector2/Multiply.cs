#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityVector2
{
    [TaskCategory("Basic/Vector2")]
    [TaskDescription("Multiply the Vector2 by a float.")]
    public class Multiply : Action
    {
        [Tooltip("The Vector2 to multiply of")]
        public SharedVector2 vector2Variable;
        [Tooltip("The value to multiply the Vector2 of")]
        public SharedFloat multiplyBy;
        [Tooltip("The multiplication resut")]
        [RequiredField]
        public SharedVector2 storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = vector2Variable.Value * multiplyBy.Value;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            vector2Variable = storeResult = Vector2.zero;
            multiplyBy = 0;
        }
    }
}