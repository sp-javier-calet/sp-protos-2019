#if !BEHAVIOR_DESIGNER_STANDALONE
using UnityEngine;
using UnityEditor;
using BehaviorDesigner.Runtime;

namespace BehaviorDesigner.Editor
{
    [CustomEditor(typeof(ExternalBehaviorTree))]
    public class ExternalBehaviorTreeInspector : ExternalBehaviorInspector
    {
        // intentionally left blank
    }
}
#endif