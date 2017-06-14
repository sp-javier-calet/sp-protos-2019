#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedObject : SharedVariable<Object>
    {
        public static explicit operator SharedObject(Object value) { return new SharedObject { mValue = value }; }
    }
}
#else
using System;
namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedObject : SharedVariable<Object>
    {
    }
}
#endif