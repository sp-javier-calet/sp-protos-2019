#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedRect : SharedVariable<Rect>
    {
        public static implicit operator SharedRect(Rect value) { return new SharedRect { mValue = value }; }
    }
}