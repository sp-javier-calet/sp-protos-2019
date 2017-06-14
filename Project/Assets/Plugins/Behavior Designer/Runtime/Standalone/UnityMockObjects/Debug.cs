using System;

namespace BehaviorDesigner.Runtime.Standalone
{   
    public static class Debug
    {
        static event Action<object> LogDelegate = null;
        static event Action<object> LogWarningDelegate = null;
        static event Action<object> LogErrorDelegate = null;
        static event Action<bool, object> AssertDelegate = null;

        public static void Log(object message)
        {
            Console.WriteLine(message);
            if(LogDelegate != null)
            {
                LogDelegate(message);
            }
        }

        public static void LogWarning(object message)
        {
            Console.WriteLine(string.Format("[Warning] {0}", message));
            if(LogWarningDelegate != null)
            {
                LogWarningDelegate(message);
            }
        }

        public static void LogError(object message)
        {
            Console.WriteLine(string.Format("[Error] {0}", message));
            if(LogErrorDelegate != null)
            {
                LogErrorDelegate(message);
            }
        }

        public static void Assert(bool condition, object message)
        {
            Assert(condition, message);
            if(AssertDelegate != null)
            {
                AssertDelegate(condition, message);
            }
        }
    }
}
