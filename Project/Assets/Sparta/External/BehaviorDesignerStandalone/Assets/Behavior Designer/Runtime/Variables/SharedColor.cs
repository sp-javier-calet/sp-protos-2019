#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedColor : SharedVariable<Color>
    {
        public static implicit operator SharedColor(Color value) { return new SharedColor { mValue = value }; }
    }
}