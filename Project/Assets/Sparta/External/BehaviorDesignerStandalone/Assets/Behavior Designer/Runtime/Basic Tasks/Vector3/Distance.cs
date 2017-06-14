#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityVector3
{
    [TaskCategory("Basic/Vector3")]
    [TaskDescription("Returns the distance between two Vector3s.")]
    public class Distance : Action
    {
        [Tooltip("The first Vector3")]
        public SharedVector3 firstVector3;
        [Tooltip("The second Vector3")]
        public SharedVector3 secondVector3;
        [Tooltip("The distance")]
        [RequiredField]
        public SharedFloat storeResult;

        public override TaskStatus OnUpdate()
        {
            storeResult.Value = Vector3.Distance(firstVector3.Value, secondVector3.Value);
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            firstVector3 = secondVector3 = Vector3.zero;
            storeResult = 0;
        }
    }
}
