#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedObjectList : SharedVariable<List<Object>>
    {
        public static implicit operator SharedObjectList(List<Object> value) { return new SharedObjectList { mValue = value }; }
    }
}
#else
using System;
using System.Collections.Generic;
namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedObjectList : SharedVariable<List<Object>>
    {
    }
}
#endif