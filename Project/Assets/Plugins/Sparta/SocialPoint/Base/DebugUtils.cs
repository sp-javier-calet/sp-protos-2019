// #defines must be placed at the start of the file #csharpfirstworldproblems
#if UNITY_5_3_OR_NEWER
#define UNITY
#endif
#if DEBUG || UNITY_EDITOR || UNITY_STANDALONE
#define TRACE
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SocialPoint.Utils;

namespace SocialPoint.Base
{
    public static class DebugUtils
    {
        [Conditional("TRACE")]
        public static void StackTrace(params object[] objs)
        {
            var sb = StringUtils.StartBuilder();
            var st = new System.Diagnostics.StackTrace();
            sb.AppendLine(st.ToString());

            if(objs.Length > 0)
            {
                sb.AppendLine("OBJECTS:");
                for(int i = 0, objsLength = objs.Length; i < objsLength; i++)
                {
                    var obj = objs[i];
                    sb.AppendLine(obj.ToString());
                }
            }

            #if UNITY
            Base.Log.i(StringUtils.FinishBuilder(sb));
            #endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string msg = "")
        {
            #if UNITY
            UnityEngine.Assertions.Assert.IsTrue(condition, msg);
            if(condition)
            {
                return;
            }

            #if UNITY_EDITOR
            try
            {
                if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    UnityEditor.EditorApplication.isPaused = true;
                }
            }
            catch(MissingMethodException)
            {
                /* This is required to run Tests from MonoDevelop, 
                         * which includes the UNITY_EDITOR flag but EditorApplication is not available. */
            }
            #endif

            #else
            if(!condition)
            {
                SocialPoint.Base.Log.e(msg);
            }
            #endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        public static void Assert(Func<bool> assertFunction, string msg = "")
        {
            Assert(assertFunction(), msg);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        public static void Assert(Func<string> assertFunction)
        {
            var error = assertFunction();
            Assert(!string.IsNullOrEmpty(error), error);
        }

        [Conditional("DEBUG")]
        public static void Break()
        {
            #if UNITY
            UnityEngine.Debug.Break();
            #endif
        }

        public static void Stop()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        public static bool IsDebugBuild
        {
            get
            {
                #if UNITY
                return UnityEngine.Debug.isDebugBuild;
                #else
                return false;
                #endif
            }
        }

        [Obsolete("Use Log class instead")]
        public static void Log(string message)
        {
            Base.Log.i(message);
        }

        [Obsolete("Use Log.e instead")]
        public static void LogError(string message)
        {
            Base.Log.e(message);
        }

        [Obsolete("Use Log.x instead")]
        public static void LogException(Exception exception)
        {
            Base.Log.x(exception);
        }

        [Obsolete("Use Log.w instead")]
        public static void LogWarning(string message)
        {
            Base.Log.w(message);
        }
    }
}

