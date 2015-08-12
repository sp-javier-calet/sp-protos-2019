using Zenject;
using UnityEngine;
using SocialPoint.Network;

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

    private void OnRequestSetup(HttpRequest req)
    {
        if(req.Proxy == null)
        {
            req.Proxy = _httpProxy;
        }
    }

    public HttpClient(MonoBehaviour mono) : base(mono)
    {
        RequestSetup += OnRequestSetup;
    }
}
