#if BEHAVIOR_DESIGNER_STANDALONE
using BehaviorDesigner.Runtime.Standalone;
#else
using UnityEngine;
#endif
using System;

namespace BehaviorDesigner.Runtime
{
    [System.Serializable]
    public class GenericVariable
    {
        [SerializeField]
        public string type = "SharedString";
        [SerializeField]
        public SharedVariable value;

        public GenericVariable()
        {
            value = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as SharedVariable;
        }
    }

    [System.Serializable]
    public class SharedGenericVariable : SharedVariable<GenericVariable>
    {
        public SharedGenericVariable()
        {
            mValue = new GenericVariable();
        }

        public static implicit operator SharedGenericVariable(GenericVariable value) { return new SharedGenericVariable { mValue = value }; }
    }
}