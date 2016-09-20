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

        #region Logger interfaces and properties

        public interface ILogger
        {
            void Log(string message);

            void LogWarning(string message);

            void LogError(string message);

            void LogException(Exception e);
        }

        public interface IBreadcrumbLogger
        {
            void Leave(string message);
        }

        /// <summary>
        /// Default logger for both logs and breadcrumbs
        /// </summary>
        readonly static InternalLogger DefaultLogger = new InternalLogger();

        /// <summary>
        /// Breadcrumb Logger
        /// </summary>
        static IBreadcrumbLogger _breadcrumbLogger = DefaultLogger;

        public static IBreadcrumbLogger BreadcrumbLogger
        {
            set
            {
                _breadcrumbLogger = value ?? DefaultLogger;
            }
        }

        /// <summary>
        /// Main Logger
        /// </summary>
        static ILogger _logger = DefaultLogger;

        public static ILogger Logger
        {
            set
            {
                _logger = value ?? DefaultLogger;
            }
        }

        #endregion

        #region Platform implementation

        #if UNITY_5

        class InternalLogger : ILogger, IBreadcrumbLogger
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

            public void LogException(Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            public void Leave(string message)
            {
                UnityEngine.Debug.Log(message);
            }
        }
      
        #else

        class InternalLogger : ILogger, IBreadcrumbLogger
        {
            public void Log(string message)
            {
                System.Console.WriteLine(message);
            }

            public void LogWarning(string message)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Warning] ", message));
            }

            public void LogError(string message)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Error] ", message));
            }

            public void LogException(Exception e)
            {
                System.Console.WriteLine(string.Format(TaggedFormat, " [Exception] ", e));
            }

            public void Leave(string message)
            {
                System.Console.WriteLine(message);
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
            _logger.Log(message);
        }

        /// <summary>
        /// Verbose Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(VerboseFlag)]
        public static void v(string tag, string message)
        {
            _logger.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Debug Log.
        /// </summary>
        [System.Diagnostics.Conditional(DebugFlag)]
        public static void d(string message)
        {
            _logger.Log(message);
        }

        /// <summary>
        /// Debug Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(DebugFlag)]
        public static void d(string tag, string message)
        {
            _logger.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Info Log.
        /// </summary>
        [System.Diagnostics.Conditional(InfoFlag)]
        public static void i(string message)
        {
            _logger.Log(message);
        }

        /// <summary>
        /// Info Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(InfoFlag)]
        public static void i(string tag, string message)
        {
            _logger.Log(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Warning Log
        /// </summary>
        [System.Diagnostics.Conditional(WarningFlag)]
        public static void w(string message)
        {
            _logger.LogWarning(message);
        }

        /// <summary>
        /// Warning Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(WarningFlag)]
        public static void w(string tag, string message)
        {
            _logger.LogWarning(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Error Log.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void e(string message)
        {
            _logger.LogError(message);
        }

        /// <summary>
        /// Error Log. Uses a tag to identify the message.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void e(string tag, string message)
        {
            _logger.LogError(string.Format(TaggedFormat, tag, message));
        }

        /// <summary>
        /// Exception Log.
        /// </summary>
        [System.Diagnostics.Conditional(ErrorFlag)]
        public static void x(Exception e)
        {
            _logger.LogException(e);
        }

        /// <summary>
        /// Breadcrumb Logs.
        /// </summary>
        public static void b(string breadcrumb)
        {
            _breadcrumbLogger.Leave(breadcrumb);
        }

        #endregion
    }
}
