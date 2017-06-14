#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityInput
{
    [TaskCategory("Basic/Input")]
    [TaskDescription("Returns success when the specified mouse button is pressed.")]
    public class IsMouseDown : Conditional
    {
        [Tooltip("The button index")]
        public SharedInt buttonIndex;

        public override TaskStatus OnUpdate()
        {
            return Input.GetMouseButtonDown(buttonIndex.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            buttonIndex = 0;
        }
    }
}
#endif