#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedQuaternion : SharedVariable<Quaternion>
    {
        public static implicit operator SharedQuaternion(Quaternion value) { return new SharedQuaternion { mValue = value }; }
    }
}