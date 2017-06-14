#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    // Wrapper for the Behavior class
    [AddComponentMenu("Behavior Designer/Behavior Tree")]
    [System.Serializable]
    public class BehaviorTree : Behavior
    {
#if BEHAVIOR_DESIGNER_STANDALONE
        public BehaviorTree(BehaviorManager behaviorManager, BinaryDeserialization binaryDeserialization) : base(behaviorManager, binaryDeserialization)
        {
        }
#endif
    }
}