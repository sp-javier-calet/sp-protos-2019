//#define DEBUG_LEVEL_LOG
//#define DEBUG_LEVEL_WARN
//#define DEBUG_LEVEL_ERROR
using UnityEngine;
using System.Collections;
using SocialPoint.Base;
using SPDebug = SocialPoint.Base.Debug;

namespace SocialPoint.Utils
{
    public static class Logger
    {
        static LoggerContext _contextInstace;

        static LoggerContext ContextInstace
        {
            get
            {
                if(_contextInstace == null)
                {
                    _contextInstace = GameObject.FindObjectOfType<LoggerContext>();
                    SPDebug.Assert(_contextInstace != null, "_contextInstace!=null");
                }

                return _contextInstace;
            }
        }

        [System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
        public static void Log(string context, System.Object message)
        {
            if(ContextInstace != null && ContextInstace.IsContextEnabled(context))
            {
                SPDebug.Log(context + ": " + message);
            }
        }

        [System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
        [System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
        public static void LogWarning(string context, System.Object message)
        {
            if(ContextInstace != null && ContextInstace.IsContextEnabled(context))
            {
                SPDebug.LogWarning(context + ": " + message);
            }
        }

        [System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
        [System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
        [System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
        public static void LogError(string context, System.Object message)
        {
            if(ContextInstace != null && ContextInstace.IsContextEnabled(context))
            {
                SPDebug.LogError(context + ": " + message);
            }
        }

        [System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
        [System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
        [System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
        public static void LogException(string context, System.Exception message)
        {
            if(ContextInstace != null && ContextInstace.IsContextEnabled(context))
            {
                SPDebug.LogException(message);
            }
        }

        private static bool AddContext(string context)
        {
            if(ContextInstace == null)
            {
                return false;
            }

            ContextInstace.AddContext(context);

            return true;
        }
    }
}
