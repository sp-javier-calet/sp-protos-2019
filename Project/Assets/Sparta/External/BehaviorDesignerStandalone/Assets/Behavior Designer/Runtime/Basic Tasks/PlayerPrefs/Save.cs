#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityPlayerPrefs
{
    [TaskCategory("Basic/PlayerPrefs")]
    [TaskDescription("Saves the PlayerPrefs.")]
    public class Save : Action
    {
        public override TaskStatus OnUpdate()
        {
            PlayerPrefs.Save();

            return TaskStatus.Success;
        }
    }
}
#endif