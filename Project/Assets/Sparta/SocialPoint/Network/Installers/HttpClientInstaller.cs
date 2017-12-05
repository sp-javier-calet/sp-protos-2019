using System;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Network
{
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

    public class HttpClientInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public string Config = "basegame";
            public bool EnableHttpStreamPinning = false;
            public bool EnableRetryHttpClient = false;
        }

        public SettingsData Settings = new SettingsData();

        string _httpProxy;
        INetworkInfo _networkInfo;

        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);

            #pragma warning disable 0162

            // Http Client
            if(Curl.IsSupported)
            {
                Container.Rebind<CurlHttpClient>().ToMethod<CurlHttpClient>(CreateCurlHttpClient);
                if(!Settings.EnableRetryHttpClient)
                {
                    Container.Rebind<IHttpClient>("internal").ToLookup<CurlHttpClient>();
                }
            }
            else
            {
                Container.Rebind<WebRequestHttpClient>().ToMethod<WebRequestHttpClient>(CreateWebRequestHttpClient);
                if(!Settings.EnableRetryHttpClient)
                {
                    Container.Rebind<IHttpClient>("internal").ToLookup<WebRequestHttpClient>();
                }
            }

            if(Settings.EnableRetryHttpClient)
            {
                Container.Rebind<RetryHttpClient>().ToMethod<RetryHttpClient>(CreateRetryHttpClient);
                Container.Rebind<IHttpClient>("internal").ToLookup<RetryHttpClient>();
            }

            Container.Bind<IDisposable>().ToLookup<IHttpClient>("internal");


            // Http Stream Client
            if(Curl.IsSupported)
            {
                Container.Bind<CurlHttpStreamClient>().ToMethod<CurlHttpStreamClient>(CreateStreamClient);    
                Container.Rebind<IHttpStreamClient>().ToLookup<CurlHttpStreamClient>(); 
                Container.Bind<IDisposable>().ToLookup<IHttpStreamClient>();

                #if ADMIN_PANEL
                Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelHttpStream>(CreateAdminPanel);
                #endif
            }

            #pragma warning restore 0162
        }

        public void Initialize()
        {
            _httpProxy = EditorProxy.GetProxy();
            _networkInfo = Container.Resolve<INetworkInfo>();
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

        RetryHttpClient CreateRetryHttpClient()
        {
            #pragma warning disable 0162

            IHttpClient client;
            if(Curl.IsSupported)
            {
                client = Container.Resolve<CurlHttpClient>();
            }
            else
            {
                client = Container.Resolve <WebRequestHttpClient>();
            }
            #pragma warning restore 0162
            var retryClient = new RetryHttpClient(client);
                             
            return retryClient;
        }

        void OnRequestSetup(HttpRequest req)
        {
            if(string.IsNullOrEmpty(req.Proxy) && !string.IsNullOrEmpty(_httpProxy))
            {
                req.Proxy = _httpProxy;
            }
            if(string.IsNullOrEmpty(req.Proxy) && _networkInfo != null && _networkInfo.Proxy != null)
            {
                req.Proxy = _networkInfo.Proxy.ToString();
            }
        }

        #if ADMIN_PANEL
        AdminPanelHttpStream CreateAdminPanel()
        {
            return new AdminPanelHttpStream(
                Container.Resolve<CurlHttpStreamClient>());
        }
        #endif
    }
}
