#if !BEHAVIOR_DESIGNER_STANDALONE
using UnityEditor;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Editor
{
    [CustomEditor(typeof(BehaviorTree))]
    public class BehaviorTreeInspector : BehaviorInspector
    {
        // intentionally left blank
    }
}
#endif