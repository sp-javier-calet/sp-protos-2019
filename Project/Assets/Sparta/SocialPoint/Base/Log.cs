#region Incremental log levels

#if SPARTA_LOG_VERBOSE
#define SPARTA_LOG_DEBUG
#endif

#if SPARTA_LOG_DEBUG
#define SPARTA_LOG_INFO
#endif

#if SPARTA_LOG_INFO
#define SPARTA_LOG_WARNING
#endif

#if SPARTA_LOG_WARNING
#define SPARTA_LOG_ERROR
#endif

#endregion

using System;

namespace SocialPoint.Base
{
    public static class Log
    {
        const string VerboseFlag = "SPARTA_LOG_VERBOSE";
        const string DebugFlag = "SPARTA_LOG_DEBUG";
        const string InfoFlag = "SPARTA_LOG_INFO";
        const string WarningFlag = "SPARTA_LOG_WARNING";
        const string ErrorFlag = "SPARTA_LOG_ERROR";

        const string TaggedFormat = "[{0}] {1}";
        const string ExceptionFormat = "{0}\nReason:\n{1}";

        #region Platform implementation

        #if UNITY_5

        static class Internal
        {
            public static void Log(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            public static void LogWarning(string message)
            {
                UnityEngine.Debug.LogWarning(message);
            }

            public static void LogError(string message)
            {
                UnityEngine.Debug.LogError(message);
            }

            public static void LogException(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
        }
      
        #else

        static class Internal
        {
            public static void Log(string message)
            {
                System.Console.WriteLine(message);
            }

            public static void LogWarning(string message)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Warning] ", message));
            }

            public static void LogError(string message)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Error] ", message));
            }

            public static void LogException(Exception e)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Exception] ", e));
            }
        }
        #endif

        #endregion

        #region Log interface

        /// <summary>
        /// Verbose Log
        /// </summary>
        [System.Diagnostics.Conditional(VerboseFlag)]
        public static void v(string message)
        {
            Internal.Log(message);
        }

        /// <summary>
        /// Verbose Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(VerboseFlag)]
        public static void v(string tag, string message)
        {
            Internal.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Debug Log.
        /// </summary>
        [System.Diagnostics.Conditional(DebugFlag)]
        public static void d(string message)
        {
            Internal.Log(message);
        }

        /// <summary>
        /// Debug Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(DebugFlag)]
        public static void d(string tag, string message)
        {
            Internal.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Info Log.
        /// </summary>
        [System.Diagnostics.Conditional(InfoFlag)]
        public static void i(string message)
        {
            Internal.Log(message);
        }

        /// <summary>
        /// Info Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(InfoFlag)]
        public static void i(string tag, string message)
        {
            Internal.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Warning Log
        /// </summary>
        [System.Diagnostics.Conditional(WarningFlag)]
        public static void w(string message)
        {
            Internal.LogWarning(message);
        }

        /// <summary>
        /// Warning Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(WarningFlag)]
        public static void w(string tag, string message)
        {
            Internal.LogWarning(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Error Log.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void e(string message)
        {
            Internal.LogError(message);
        }

        /// <summary>
        /// Error Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void e(string tag, string message)
        {
            Internal.LogError(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Exception Log.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void x(Exception e)
        {
            Internal.LogException(e);
        }

        #endregion
    }
}
