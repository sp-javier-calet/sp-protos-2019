#if !UNITY_EDITOR_WIN
#define CURL_SUPPORTED
#endif

using Zenject;
using UnityEngine;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

public class HttpClient : 
    #if !CURL_SUPPORTED
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

    [InjectOptional("pin_ssl_certificate")]
    byte[] certificate
    {
        set
        {
            CurlBridge.SPUnityCurlSetCertificate(value, value.Length);
        }
    }


    [Inject]
    IDeviceInfo deviceInfo;

    #if CURL_SUPPORTED
    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }
    #endif

    public HttpClient(MonoBehaviour mono):
    base(mono)
    {
        RequestSetup += OnRequestSetup;
    }

    private void OnRequestSetup(HttpRequest req)
    {
        if(string.IsNullOrEmpty(req.Proxy) && !string.IsNullOrEmpty(_httpProxy))
        {
            req.Proxy = _httpProxy;
        }
        if(string.IsNullOrEmpty(req.Proxy) && deviceInfo.NetworkInfo.Proxy != null)
        {
            req.Proxy = deviceInfo.NetworkInfo.Proxy.ToString();
        }
    }
}
