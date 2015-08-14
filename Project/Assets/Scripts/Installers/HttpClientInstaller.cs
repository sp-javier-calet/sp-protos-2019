using System;
using System.Collections;
using Zenject;
using SocialPoint.Network;

public class HttpClientInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string Proxy = string.Empty;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{
    	Container.BindInstance("http_proxy", Settings.Proxy);
        Container.Bind<IHttpClient>().ToSingle<HttpClient>();
	}
}
