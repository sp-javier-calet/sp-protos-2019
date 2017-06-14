#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityPlayerPrefs
{
    [TaskCategory("Basic/PlayerPrefs")]
    [TaskDescription("Retruns success if the specified key exists.")]
    public class HasKey : Conditional
    {
        [Tooltip("The key to check")]
        public SharedString key;

        public override TaskStatus OnUpdate()
        {
            return PlayerPrefs.HasKey(key.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            key = "";
        }
    }
}
#endif