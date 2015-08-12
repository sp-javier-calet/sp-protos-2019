using System;
using System.Collections;
using Zenject;

public class HttpClientInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public string Proxy = null;
    };
    
    public SettingsData Settings;

	public override void InstallBindings()
	{
		if(Settings.Proxy != null)
		{
        	Container.BindInstance("http_proxy", Settings.Proxy);
		}

        Container.BindAllInterfacesToSingle<HttpClient>();
	}
}
