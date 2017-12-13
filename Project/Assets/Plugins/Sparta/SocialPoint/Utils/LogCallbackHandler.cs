using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Utils
{
    /// <summary>
    /// Unity only handles a single delegate registered with Application.RegisterLogCallback
    /// http://feedback.unity3d.com/suggestions/change-application-dot-registerlogcallback-to-allow-multiple-callbacks
    /// 
    /// This class is used to work around that by allowing multiple delegates to hook to the log callback.
    /// source: http://blog.dreasgrech.com/2014/07/a-workaround-for-allowing-multiple.html 
    /// </summary>
    public static class LogCallbackHandler
    {
        static readonly List<Application.LogCallback> callbacks;

        static LogCallbackHandler()
        {
            callbacks = new List<Application.LogCallback>();
        
            Application.logMessageReceived += HandleLog;
        }

        /// <summary>
        /// Register a delegate to be called on log messages.
        /// </summary>
        /// <param name="logCallback"></param>
        public static void RegisterLogCallback(Application.LogCallback logCallback)
        {
            callbacks.Add(logCallback);
        }

        public static void UnregisterLogCallback(Application.LogCallback logCallback)
        {
            callbacks.Remove(logCallback);
        }

        static void HandleLog(string condition, string stackTrace, LogType type)
        {
            for(var i = 0; i < callbacks.Count; i++)
            {
                var callback = callbacks[i];
                if(callback != null)
                {
                    callback(condition, stackTrace, type);
                }
            }
        }
    }
}
