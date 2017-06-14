#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedVector3 : SharedVariable<Vector3>
    {
        public static implicit operator SharedVector3(Vector3 value) { return new SharedVector3 { mValue = value }; }
    }
}