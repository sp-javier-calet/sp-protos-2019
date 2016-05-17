using System;
using System.Collections.Generic;
using SocialPoint.AssetSerializer.Exceptions;
using UnityEngine;

namespace SocialPoint.AssetSerializer.Utils
{
    public static class SerializerLogger
    {
        static bool Enabled;

        static List<string> SerializationLogs = new List<string>();
        static List<string> SerializationWarnings = new List<string>();
        static List<string> SerializationErrors = new List<string>();

        static string ScenePrefix;

        static readonly List<KeyValuePair<string, string>> NestedNodePrefix = new List<KeyValuePair<string, string>>();

        static SerializerLogger()
        {
            Enabled = false;
            Clear();
        }

        public static void Clear()
        {
            SerializationLogs.Clear();
            SerializationWarnings.Clear();
            SerializationErrors.Clear();
            ScenePrefix = "";
            NestedNodePrefix.Clear();
        }

        public static void Enable(bool value)
        {
            Enabled = value;
        }

        public static bool HasErrorLogs()
        {
            return SerializationErrors.Count > 0;
        }

        public static void SetCurrentScene(string sceneName)
        {
            ScenePrefix = sceneName;
        }

        public static void AddCurrentGameObject(string goName)
        {
            NestedNodePrefix.Add(new KeyValuePair<string, string>("GameObject", goName));
        }

        public static void AddCurrentComponent(string compName)
        {
            NestedNodePrefix.Add(new KeyValuePair<string, string>("Component", compName));
        }

        public static void RemoveNode()
        {
            NestedNodePrefix.RemoveAt(NestedNodePrefix.Count - 1);
        }

        public static void LogMsg(string msg)
        {
            if(Enabled)
            {
                SerializationLogs.Add(WriteLog(msg));
            }
        }

        public static void LogWarning(string msg)
        {
            if(Enabled)
            {
                SerializationWarnings.Add(WriteLog(msg));
            }
        }

        public static void LogError(string msg)
        {
            if(Enabled)
            {
                SerializationErrors.Add(WriteLog(msg));
            }
        }

        public static void ShowLogAndExceptIfNeeded()
        {
            for(int i = 0, SerializationLogsCount = SerializationLogs.Count; i < SerializationLogsCount; i++)
            {
                string log = SerializationLogs[i];
                Debug.Log(log);
            }

            for(int i = 0, SerializationWarningsCount = SerializationWarnings.Count; i < SerializationWarningsCount; i++)
            {
                string log = SerializationWarnings[i];
                Debug.LogWarning(log);
            }

            for(int i = 0, SerializationErrorsCount = SerializationErrors.Count; i < SerializationErrorsCount; i++)
            {
                string log = SerializationErrors[i];
                Debug.LogError(log);
            }

            if(SerializationErrors.Count > 0)
            {
                throw new SerializationProcessException(SerializationErrors);
            }
        }

        static string WriteLog(string msg)
        {
            string fullMsg = "";
            if(!ScenePrefix.Equals(String.Empty))
            {
                fullMsg += String.Format("On Scene {0}", ScenePrefix);
            }

            for(int i = 0, NestedNodePrefixCount = NestedNodePrefix.Count; i < NestedNodePrefixCount; i++)
            {
                KeyValuePair<string, string> node = NestedNodePrefix[i];
                fullMsg += String.Format(" - ({0}, {1})", node.Key, node.Value);
            }

            fullMsg += ": " + msg;

            return fullMsg;
        }
    }
}
