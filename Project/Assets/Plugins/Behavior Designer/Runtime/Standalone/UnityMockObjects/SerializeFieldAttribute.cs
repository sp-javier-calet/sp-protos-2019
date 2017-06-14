using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class SerializeField : Attribute
    {
        public SerializeField()
        {
            // Do nothing in standalone mode
        }
    }
}
