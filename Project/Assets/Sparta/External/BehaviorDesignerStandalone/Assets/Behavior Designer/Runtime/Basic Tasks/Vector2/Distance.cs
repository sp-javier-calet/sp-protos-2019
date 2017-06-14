#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityVector2
{
    [TaskCategory("Basic/Vector2")]
    [TaskDescription("Returns the distance between two Vector2s.")]
    public class Distance : Action
    {
        [Tooltip("The first Vector2")]
        public SharedVector2 firstVector2;
        [Tooltip("The second Vector2")]
        public SharedVector2 secondVector2;
        [Tooltip("The distance")]
        [RequiredField]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = Vector2.Distance(firstVector2.Value, secondVector2.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            firstVector2 = secondVector2 = Vector2.zero;
            storeResult = 0;
        }
    }
}