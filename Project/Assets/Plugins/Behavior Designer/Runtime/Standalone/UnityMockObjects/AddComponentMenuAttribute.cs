using System;

namespace BehaviorDesigner.Runtime.Standalone
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AddComponentMenu : Attribute
    {
        public AddComponentMenu(string label)
        {
            // Do nothing in standalone mode
        }
    }
}
