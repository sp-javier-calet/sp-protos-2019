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
        public bool EnableHttpStreamPinning = false;
    }

    public SettingsData Settings = new SettingsData();

    string _httpProxy;
    IDeviceInfo _deviceInfo;

    public override void InstallBindings()
    {
        _httpProxy = EditorProxy.GetProxy();
        _deviceInfo = Container.Resolve<IDeviceInfo>();

        // Http Client
        if(Curl.IsSupported)
        {
            Container.Rebind<CurlHttpClient>().ToMethod<CurlHttpClient>(CreateCurlHttpClient);
            Container.Rebind<IHttpClient>("internal").ToLookup<CurlHttpClient>();
            Container.Rebind<IHttpClient>().ToLookup<CurlHttpClient>();
        }
        else
        {
            Container.Rebind<WebRequestHttpClient>().ToMethod<WebRequestHttpClient>(CreateWebRequestHttpClient);
            Container.Rebind<IHttpClient>("internal").ToLookup<WebRequestHttpClient>();
            Container.Rebind<IHttpClient>().ToLookup<WebRequestHttpClient>(); 
        }

        Container.Bind<IDisposable>().ToLookup<IHttpClient>();

        // Http Stream Client
        if(Curl.IsSupported)
        {
            Container.Bind<CurlHttpStreamClient>().ToMethod<CurlHttpStreamClient>(CreateStreamClient);    
            Container.Rebind<IHttpStreamClient>().ToLookup<CurlHttpStreamClient>(); 
            Container.Bind<IDisposable>().ToLookup<IHttpStreamClient>();

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHttpStream>(CreateAdminPanel);
        }
    }

    CurlHttpClient CreateCurlHttpClient()
    {
        var client = new CurlHttpClient(
                         Container.Resolve<ICoroutineRunner>()
                     );

        client.AppEvents = Container.Resolve<IAppEvents>();

        client.RequestSetup += OnRequestSetup;
        client.Config = Settings.Config;
        return client;
    }

    WebRequestHttpClient CreateWebRequestHttpClient()
    {
        var client = new WebRequestHttpClient(
                         Container.Resolve<ICoroutineRunner>()
                     );

        client.RequestSetup += OnRequestSetup;
        client.Config = Settings.Config;
        return client;
    }

    CurlHttpStreamClient CreateStreamClient()
    {
        var client = new CurlHttpStreamClient(
                         Container.Resolve<ICoroutineRunner>(),
                         Container.Resolve<IAppEvents>()
                     );

        client.RequestSetup += OnRequestSetup;
        if(Settings.EnableHttpStreamPinning)
        {
            client.Config = Settings.Config;
        }
        return client;
    }

    void OnRequestSetup(HttpRequest req)
    {
        if(string.IsNullOrEmpty(req.Proxy) && !string.IsNullOrEmpty(_httpProxy))
        {
            req.Proxy = _httpProxy;
        }
        if(string.IsNullOrEmpty(req.Proxy) && _deviceInfo.NetworkInfo.Proxy != null)
        {
            req.Proxy = _deviceInfo.NetworkInfo.Proxy.ToString();
        }
    }

    AdminPanelHttpStream CreateAdminPanel()
    {
        return new AdminPanelHttpStream(
            Container.Resolve<CurlHttpStreamClient>());
    }
}
