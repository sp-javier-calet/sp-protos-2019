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

        Container.BindInstance("http_client_editor_proxy", Settings.EditorProxy);
        Container.Bind<IHttpClient>().ToSingle<HttpClient>();
        Container.Bind<IDisposable>().ToSingle<HttpClient>();
	}
}
