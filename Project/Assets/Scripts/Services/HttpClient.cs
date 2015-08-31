using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;
using SocialPoint.QualityStats;

public class HttpClient : QualityStatsHttpClient
{
    private string _httpProxy;

    [InjectOptional("http_client_proxy")]
    public string InjectHttpProxy
    {
        set
        {
            _httpProxy = value;
        }
    }

    [Inject]
    public IDeviceInfo DeviceInfo;

    [Inject]
    public IAppEvents AppEvents
    {
        set
        {
            value.WillGoBackground += OnWillGoBackground;
            value.WasOnBackground += OnWasOnBackground;
        }
    }

    public HttpClient(MonoBehaviour mono):
    base(new CurlHttpClient(mono))
    {
        RequestSetup += OnRequestSetup;
    }


    private void OnRequestSetup(HttpRequest req)
    {
        if(string.IsNullOrEmpty(req.Proxy))
        {
            req.Proxy = _httpProxy;
        }
        if(string.IsNullOrEmpty(req.Proxy) && DeviceInfo.NetworkInfo.Proxy != null)
        {
            req.Proxy = DeviceInfo.NetworkInfo.Proxy.ToString();
        }
    }

    private void OnWillGoBackground()
    {
        OnApplicationPause(true);
    }
    
    private void OnWasOnBackground()
    {
        OnApplicationPause(false);
    }   
}
