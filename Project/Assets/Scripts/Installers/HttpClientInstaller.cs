using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Network;
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

        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHttpClient>(CreateAdminPanel);
    }

    HttpClient CreateHttpClient()
    {
        string proxy = EditorProxy.GetProxy();
        var client = new HttpClient(
            Container.Resolve<ICoroutineRunner>(), proxy,
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAppEvents>()
        );
        client.Config = Settings.Config;
        return client;
    }

    AdminPanelHttpClient CreateAdminPanel()
    {
        return new AdminPanelHttpClient(
            Container.Resolve<ICoroutineRunner>());
    }
}
