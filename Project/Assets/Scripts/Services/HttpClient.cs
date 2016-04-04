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
    IDeviceInfo deviceInfo;

    public HttpClient(ICoroutineRunner runner):
    base(runner)
    {
        RequestSetup += OnRequestSetup;
        _httpProxy = ServiceLocator.Instance.TryResolve<string>("http_client_proxy");
        Config = ServiceLocator.Instance.TryResolve<string>("http_client_config");
        deviceInfo = ServiceLocator.Instance.Resolve<IDeviceInfo>();
        #if CURL_SUPPORTED
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        #endif
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
