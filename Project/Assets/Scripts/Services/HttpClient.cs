using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

public class HttpClient : CurlHttpClient
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

    public HttpClient(MonoBehaviour mono) : base(mono)
    {
        RequestSetup += OnRequestSetup;
    }
}
