using System;
using System.Collections;
using Zenject;
using SocialPoint.QualityStats;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;
using SocialPoint.Network;

public class QualityStatsInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {

    };
    
    public SettingsData Settings;

    [Inject]
    IDeviceInfo DeviceInfo;

    [Inject]
    IAppEvents AppEvents;

    [Inject]
    IHttpClient HttpClient;

	public override void InstallBindings()
	{
        var httpClient = new QualityStatsHttpClient(HttpClient);
        Container.Rebind<IHttpClient>().ToInstance(httpClient);

        var stats = new SocialPointQualityStats(DeviceInfo, AppEvents);
        stats.AddQualityStatsHttpClient(httpClient);
        Container.BindInstance(stats);
	}
}
