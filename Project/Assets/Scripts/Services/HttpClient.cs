#if !UNITY_EDITOR_WIN
#define CURL_SUPPORTED
#endif

using SocialPoint.Dependency;
using SocialPoint.Utils;
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
    string _httpProxy;
    IDeviceInfo _deviceInfo;

    public HttpClient(ICoroutineRunner runner, string proxy, IDeviceInfo deviceInfo, IAppEvents appEvents) :
        base(runner)
    {
        RequestSetup += OnRequestSetup;
        _httpProxy = proxy;
        _deviceInfo = deviceInfo;
        #if CURL_SUPPORTED
        AppEvents = appEvents;
        #endif
    }

    private void OnRequestSetup(HttpRequest req)
    {
        if(string.IsNullOrEmpty(req.Proxy) && !string.IsNullOrEmpty(_httpProxy))
        {
            req.Proxy = _httpProxy;
        }
        if(string.IsNullOrEmpty(req.Proxy) && _deviceInfo.NetworkInfo.Proxy != null)
        {
            req.Proxy = _deviceInfo.NetworkInfo.Proxy.ToString();
        }
    }
}
