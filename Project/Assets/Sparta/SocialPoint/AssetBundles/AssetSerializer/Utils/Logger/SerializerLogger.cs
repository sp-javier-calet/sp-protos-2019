using UnityEngine;
using System;
using System.Collections.Generic;
using SocialPoint.AssetSerializer.Exceptions;


namespace SocialPoint.AssetSerializer.Utils
{
    public static class SerializerLogger
    {
        static bool Enabled;

        static List<string> SerializationLogs = new List<string>();
        static List<string> SerializationWarnings = new List<string>();
        static List<string> SerializationErrors = new List<string>();

        static string ScenePrefix;

        static List<KeyValuePair<string, string>> NestedNodePrefix = new List<KeyValuePair<string, string>>();

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
            foreach(string log in SerializationLogs)
            {
                Debug.Log(log);
            }

            foreach(string log in SerializationWarnings)
            {
                Debug.LogWarning(log);
            }

            foreach(string log in SerializationErrors)
            {
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
			
            foreach(KeyValuePair<string, string> node in NestedNodePrefix)
            {
                fullMsg += String.Format(" - ({0}, {1})", node.Key, node.Value);
            }
			
            fullMsg += ": " + msg;

            return fullMsg;
        }
    }
}
