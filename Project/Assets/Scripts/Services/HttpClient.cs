using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

public class HttpClient : 
    #if UNITY_EDITOR_WIN
    WebRequestHttpClient
        #else
        CurlHttpClient
        #endif
{
    private string _httpProxy;

    [InjectOptional("http_client_proxy")]
    string injectHttpProxy
    {
        set
        {
            _httpProxy = value;
        }
    }

    [InjectOptional("pinn_ssl_certificate")]
    byte[] certificate
    {
        set
        {
            CurlBridge.SPUnityCurlSetCertificate(value, value.Length);
        }
    }


    [Inject]
    IDeviceInfo deviceInfo;

    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    public HttpClient(MonoBehaviour mono):
    base(mono)
    {
        RequestSetup += OnRequestSetup;
    }

    private void OnRequestSetup(HttpRequest req)
    {
        if(string.IsNullOrEmpty(req.Proxy))
        {
            req.Proxy = _httpProxy;
        }
        if(string.IsNullOrEmpty(req.Proxy) && deviceInfo.NetworkInfo.Proxy != null)
        {
            req.Proxy = deviceInfo.NetworkInfo.Proxy.ToString();
        }
    }
}
