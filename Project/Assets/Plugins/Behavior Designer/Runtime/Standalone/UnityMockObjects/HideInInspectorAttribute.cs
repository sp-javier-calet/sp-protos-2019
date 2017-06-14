using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class HideInInspector : Attribute
    {
        public HideInInspector()
        {
            // Do nothing in standalone mode
        }
    }
}
