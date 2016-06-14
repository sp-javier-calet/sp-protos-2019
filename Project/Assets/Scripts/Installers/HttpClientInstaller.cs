using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;


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

public class HttpClientInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public string Config = "basegame";
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Rebind<HttpClient>().ToMethod<HttpClient>(CreateHttpClient);
        Container.Rebind<IHttpClient>("internal").ToLookup<HttpClient>();
        Container.Rebind<IHttpClient>().ToLookup<HttpClient>();
        Container.Bind<IDisposable>().ToLookup<IHttpClient>();
    }

    HttpClient CreateHttpClient()
    {
        string proxy = null;
        #if UNITY_EDITOR
        var proxyPath = FileUtils.Combine(Application.dataPath, "../.proxy");
        if(FileUtils.ExistsFile(proxyPath))
        {
            proxy = FileUtils.ReadAllText(proxyPath).Trim();
            DebugUtils.Log(string.Format("Using editor proxy '{0}'", proxy));
        }
        #endif

        var client = new HttpClient(
            Container.Resolve<ICoroutineRunner>(), proxy,
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAppEvents>()
        );
        client.Config = Settings.Config;
        return client;
    }
}
