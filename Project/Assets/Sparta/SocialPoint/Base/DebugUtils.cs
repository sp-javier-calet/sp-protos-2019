// #defines must be placed at the start of the file #csharpfirstworldproblems
#if UNITY_5
#define UNITY
#endif
#if DEBUG || UNITY_EDITOR || UNITY_STANDALONE
#define TRACE
#endif

using System;
using System.Diagnostics;
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
                Log.i(StringUtils.FinishBuilder(sb));
            #endif
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string msg = "")
        {
            #if UNITY
                UnityEngine.Assertions.Assert.IsTrue(condition, msg);

                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying &= condition;
                #endif

            #else
                Log.e(msg);
            #endif
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
    }
}
