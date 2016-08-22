#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif
#if UNITY_DEVICE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
#define CURL_SUPPORTED
#endif

using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.Hardware;

public class HttpClient : 
    #if CURL_SUPPORTED
    CurlHttpClient
    #else
    WebRequestHttpClient
    #endif
{
    readonly string _httpProxy;
    readonly IDeviceInfo _deviceInfo;

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

    void OnRequestSetup(HttpRequest req)
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
