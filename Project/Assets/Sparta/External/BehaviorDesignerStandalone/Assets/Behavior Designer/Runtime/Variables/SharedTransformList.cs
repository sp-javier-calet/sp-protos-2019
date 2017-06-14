#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;

using System.Collections.Generic;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedTransformList : SharedVariable<List<Transform>>
    {
        public static implicit operator SharedTransformList(List<Transform> value) { return new SharedTransformList { mValue = value }; }
    }
}
#endif