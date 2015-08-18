using System;
using System.Collections;
using Zenject;
using SocialPoint.Network;

public class HttpClientInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string EditorProxy = string.Empty;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{
#if UNITY_EDITOR
        Container.BindInstance("http_client_proxy", Settings.EditorProxy);
#endif
        Container.Bind<IHttpClient>().ToSingle<HttpClient>();
        Container.Bind<IDisposable>().ToSingle<HttpClient>();
	}
}
