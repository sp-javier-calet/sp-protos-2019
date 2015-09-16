// #defines must be placed at the start of the file #csharpfirstworldproblems
#if UNITY_4 || UNITY_5
#define UNITY
#endif
#if DEBUG || UNITY_EDITOR || UNITY_STANDALONE
    #define TRACE
#endif
using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime;
using System.Reflection;
using System.Text;

namespace SocialPoint.Base
{
    public interface IDebugLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception exception);
    }

#if UNITY
    public class UnityDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
        
        public void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void LogException(Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }
    }

#endif

    public static class DebugUtils
    {
        static DebugUtils()
        {
            #if UNITY
            Logger = new UnityDebugLogger();
            #endif
        }

        [Conditional("TRACE")]
        public static void StackTrace(params object[] objs)
        {
            var sb = new StringBuilder();
            var st = new System.Diagnostics.StackTrace();
            sb.AppendLine(st.ToString());

            if(objs.Length > 0)
            {
                sb.AppendLine("OBJECTS:");
                foreach(var obj in objs)
                {
                    sb.AppendLine(obj.ToString());
                }
            }
            #if UNITY
            UnityEngine.Debug.Log(sb.ToString());
            #endif
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string msg = "")
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            if(!condition)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                LogError(msg);
            }
#elif UNITY
            UnityEngine.Assertions.Assert.IsTrue(condition, msg);
#else
            LogError(msg);
#endif
        }

        public static IDebugLogger Logger;

        public static void Log(string message)
        {
            if(Logger != null)
            {
                Logger.Log(message);
            }
        }

        
        public static void LogError(string message)
        {
            if(Logger != null)
            {
                Logger.LogError(message);
            }
        }        

        public static void LogException(Exception exception)
        {
            if(Logger != null)
            {
                Logger.LogException(exception);
            }
        }

        public static void LogWarning(string message)
        {
            if(Logger != null)
            {
                Logger.LogWarning(message);
            }
        }

    }
}
