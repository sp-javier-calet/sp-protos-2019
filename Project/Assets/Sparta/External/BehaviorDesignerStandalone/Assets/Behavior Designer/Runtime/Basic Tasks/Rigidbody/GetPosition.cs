#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityRigidbody
{
    [TaskCategory("Basic/Rigidbody")]
    [TaskDescription("Stores the position of the Rigidbody. Returns Success.")]
    public class GetPosition : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [Tooltip("The position of the Rigidbody")]
        [RequiredField]
        public SharedVector3 storeValue;

        // cache the rigidbody component
        private Rigidbody rigidbody;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject) {
                rigidbody = currentGameObject.GetComponent<Rigidbody>();
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (rigidbody == null) {
                Debug.LogWarning("Rigidbody is null");
                return TaskStatus.Failure;
            }

            storeValue.Value = rigidbody.position;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            storeValue = Vector3.zero;
        }
    }
}
#endif