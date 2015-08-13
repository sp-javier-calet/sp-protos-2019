// #defines must be placed at the start of the file #csharpfirstworldproblems
#if DEBUG || UNITY_EDITOR || UNITY_STANDALONE
    #define TRACE
#endif
using System;
using System.Linq;
using UnityEngine;
using BaseDebug = UnityEngine.Debug;

namespace SocialPoint.Base
{
    public static class Debug
    {
        /// <summary>
        ///     Logs to the unity console a trace with the name of the current method being executed,
        ///     the method declaring type, the parameter types for that method and optionally the         
        ///     values of those parameters
        /// </summary>
        /// <param name="parameterValues">
        ///     The parameters passed as values to be printed on the log.
        /// </param>
        /// <remarks>
        ///     This method uses reflection and won't be compiled in mobile builds (ConditionalAtribute rocks!)
        /// </remarks>
        [System.Diagnostics.Conditional("TRACE")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void TraceCurrentMethod(params object[] parameterValues)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame sf = st.GetFrame(1);

            System.Reflection.MethodBase method = sf.GetMethod();
            
            LogMethodAndParameterValues(UnityEngine.Debug.Log, method.DeclaringType, method, parameterValues);
        }
        
        /// <summary>
        ///     Logs a trace with the name of the current method being executed, the method
        ///     declaring type, the parameter types for that method and optionally the values 
        ///     of those parameters
        /// </summary>
        /// <param name="parameterValues">
        ///     The parameters passed as values to be printed on the log.
        /// </param>
        /// <remarks>
        ///     This method uses reflection and won't be compiled in mobile builds (ConditionalAtribute rocks!)
        /// </remarks>
        /// <param name="logDelegate">delegate that actually performs the logging</param>
        [System.Diagnostics.Conditional("TRACE")]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void TraceCurrentMethod(System.Action<string> logDelegate, params object[] parameterValues)
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame sf = st.GetFrame(1);

            System.Reflection.MethodBase method = sf.GetMethod();
            
            LogMethodAndParameterValues(logDelegate, method.DeclaringType, method, parameterValues);

        }
        
        [System.Diagnostics.Conditional("TRACE")]
        internal static void LogMethodAndParameterValues(
            System.Action<string> logDelegate, 
            System.Type declaringType, 
            System.Reflection.MethodBase method, 
            params object[] parameterValues)
        {
            // Print the parameter values with a pretty format
            var sb = new System.Text.StringBuilder();
            
            if(parameterValues != null && parameterValues.Length > 0)
            {                
                sb.AppendLine().AppendLine("Parameter values:");
                
                for(int idx = 0; idx < parameterValues.Length; ++idx)
                {
                    sb.AppendLine().AppendFormat("[{0}] {1}", idx, parameterValues[idx]);
                      
                }
            }

            logDelegate(string.Format("[TRACE] [{3}] {0} :: {1}{2}",
                declaringType.FullName,
                method,
                sb,
                UnityEngine.Time.frameCount)); 
        }
        //[Conditional ("DEBUG")]
        public static void Assert(bool condition, string msg = "")
        {
            if(!condition)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                Debug.LogError(msg);
            }
        }

        public static void StackTrace(params object[] list)
        {
            string output = "";

            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
            output += st.ToString();

            if(list.Length > 0)
            {
                output += "\nOBJECTS:";
                for(int k = 0; k < list.Count(); k++)
                {
                    output += "\n" + list[k].ToString();
                }
            }
            output += "\n";
            BaseDebug.Log(output);
        }

        public static void Break()
        {
            BaseDebug.Break();
        }

        public static void ClearDeveloperConsole()
        {
            BaseDebug.ClearDeveloperConsole();
        }

        public static void DrawLine(Vector3 start, Vector3 end, Color color = new Color(), float duration = 0.0f, bool depthTest = true)
        {
            BaseDebug.DrawLine(start, end, color, duration, depthTest);
        }

        public static void DrawRay(Vector3 start, Vector3 dir, Color color = new Color(), float duration = 0.0f, bool depthTest = true)
        {
            BaseDebug.DrawRay(start, dir, color, duration, depthTest);
        }

        public static void Log(object message)
        {
            BaseDebug.Log(message);
        }

        public static void Log(object message, UnityEngine.Object context)
        {
            BaseDebug.Log(message, context);
        }

        public static void LogError(object message)
        {
            BaseDebug.LogError(message);
        }
        
        public static void LogError(object message, UnityEngine.Object context)
        {
            BaseDebug.LogError(message, context);
        }

        public static void LogException(Exception exception)
        {
            BaseDebug.LogException(exception);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            BaseDebug.LogException(exception, context);
        }

        public static void LogWarning(object message)
        {
            BaseDebug.LogWarning(message);
        }
        
        public static void LogWarning(object message, UnityEngine.Object context)
        {
            BaseDebug.LogWarning(message, context);
        }
    }
}
