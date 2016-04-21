using System.Reflection;
using System;
using System.Linq;

namespace SocialPoint.Tool.Server
{
    public static class ToolServiceDelegateFactory
    {
        public static ToolServiceDelegate create(string commandName)
        {
            string delegateCommandName = commandName + "Delegate";
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type currentType = currentAssembly.GetTypes().SingleOrDefault(t => t.Name == delegateCommandName);
            return (ToolServiceDelegate)Activator.CreateInstance(currentType);
        }

        /**
         * Try to find a custom parameters class for the delegate or use the default ToolServiceParameters
         */
        public static ToolServiceParameters parseParameters(string commandName, string jsonContents)
        {
            string parametersCommandName = commandName + "Parameters";
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Type currentType = currentAssembly.GetTypes().SingleOrDefault(t => t.Name == parametersCommandName);
            if (currentType != null)
            {
                return ToolServiceParameters.Instantiate (jsonContents, currentType);
            }
            else
            {
                return ToolServiceParameters.Instantiate (jsonContents, typeof(ToolServiceParameters));
            }
        }
    }
}
