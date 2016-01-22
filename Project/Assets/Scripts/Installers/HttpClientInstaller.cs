using System;
using System.Collections;
using Zenject;
using SocialPoint.Network;
using UnityEngine;
using SocialPoint.IO;
using SocialPoint.Base;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(HttpClientInstaller))]
public class HttpClientInstallerEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("The config value is passed to the native code to set the corresponding pinned SSL certificate.", MessageType.Info, true);
        EditorGUILayout.HelpBox("To setup a editor http proxy, create a \"Project/.proxy\" file containing server and port. This file is ignored so it won't affect other developers.", MessageType.Info, true);
    }
}
#endif

public class HttpClientInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string Config = "basegame";
    };
        
    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        var proxyPath = FileUtils.Combine(Application.dataPath, "../.proxy");
        if(FileUtils.Exists(proxyPath))
        {
            var proxy = FileUtils.ReadAllText(proxyPath);
            DebugUtils.Log(string.Format("Using editor proxy '{0}'", proxy));
            Container.BindInstance("http_client_proxy", proxy);
        }
#endif
        Container.BindInstance("http_client_config", Settings.Config);
        Container.Rebind<IHttpClient>().ToSingle<HttpClient>();
        Container.Bind<IDisposable>().ToLookup<IHttpClient>();
    }
}
