using UnityEngine;

namespace SocialPoint.XCodeEditor
{
    public static class XCDebug
    {
        public enum LogType
        {
            Info,
            Warning,
            Error
        }

        public delegate void Logger(LogType type,object msg);

        private static Logger _logger = (LogType type, object msg) => {
            Debug.Log(msg);
        };

        public static void setLogger(Logger logger)
        {
            _logger = logger;
        }

        public static void Log(object msg)
        {
            _logger(LogType.Info, msg);
        }

        public static void LogWarning(object msg)
        {
            _logger(LogType.Warning, msg);
        }

        public static void LogError(object msg)
        {
            _logger(LogType.Error, msg);
        }
    }
}

