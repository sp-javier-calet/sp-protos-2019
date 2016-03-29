using UnityEngine;
using UnityEditor;
using System.IO;

namespace SpartaTools.Editor.View
{
    public class ProxySettingsWindow : EditorWindow
    {
        const string ProxyEnabledKey = "SpartaEditorProxyEnabled";
        const string ProxyAddressKey = "SpartaEditorProxyAddress";
        const string ProxyPortKey = "SpartaEditorProxyPort";

        const string ProxySettingsFile = ".proxy";
        const string DefaultAddress = "localhost";
        const string DefaultPort = "8888";

        [MenuItem("Sparta/Editor proxy...", false, 300)]
        public static void OpenProxySettings()
        {
            EditorWindow.GetWindow(typeof(ProxySettingsWindow), false, "Editor proxy", true);
        }

        class ProxyData
        {
            public string Address;
            public string Port;
            public bool Enabled;

            public override string ToString()
            {
                return Address + ":" + Port;
            }
        }

        ProxyData CurrentProxy;
        Vector2 _scrollPosition;

        ProxyData Load()
        {
            var data = new ProxyData();
            data.Enabled = EditorPrefs.GetBool(ProxyEnabledKey, false);
            data.Address = EditorPrefs.GetString(ProxyAddressKey, DefaultAddress);
            data.Port = EditorPrefs.GetString(ProxyPortKey, DefaultPort);

            return data;
        }

        void Save(ProxyData data)
        {
            EditorPrefs.SetBool(ProxyEnabledKey, data.Enabled);
            EditorPrefs.SetString(ProxyAddressKey, data.Address);
            EditorPrefs.SetString(ProxyPortKey, data.Port);
        }

        void SetProxy(ProxyData data)
        {
            var path = Path.Combine(SpartaProject.Project.BasePath, ProxySettingsFile);
            if(data.Enabled)
            {
                File.WriteAllText(path, data.ToString());
            }
            else if(File.Exists(path))
            {
                File.Delete(path);
            }
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if(CurrentProxy == null)
            {
                CurrentProxy = Load();
            }

            EditorGUILayout.LabelField("Editor Proxy Settings", EditorStyles.boldLabel);
            var address = EditorGUILayout.TextField("Address", CurrentProxy.Address);
            var port = EditorGUILayout.TextField("Port", CurrentProxy.Port);
            var proxyEnabled = EditorGUILayout.Toggle("Enabled", CurrentProxy.Enabled);

            var statusChanged = CurrentProxy.Enabled != proxyEnabled;

            if(statusChanged || CurrentProxy.Port != port || CurrentProxy.Address != address)
            {
                CurrentProxy.Address = address;
                CurrentProxy.Port = port;
                CurrentProxy.Enabled = proxyEnabled;

                Save(CurrentProxy);
                SetProxy(CurrentProxy);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}