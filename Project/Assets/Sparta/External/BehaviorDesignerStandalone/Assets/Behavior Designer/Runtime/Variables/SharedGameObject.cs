#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
using GameObject = BehaviorDesigner.Runtime.Standalone.BehaviorGameObject;
#else
using UnityEngine;
#endif

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class SharedGameObject : SharedVariable<GameObject>
    {
        public static implicit operator SharedGameObject(GameObject value) { return new SharedGameObject { mValue = value }; }
    }
}