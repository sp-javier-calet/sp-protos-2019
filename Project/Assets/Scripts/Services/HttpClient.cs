using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;

public class HttpClient : CurlHttpClient
{
    private string _httpProxy;

    [InjectOptional("http_proxy")]
    public string InjectHttpProxy
    {
        set
        {
            _httpProxy = value;
        }
    }

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
