namespace SocialPoint.Utils
{
    public class NativeUtils
    {
        static INativeUtils _nativeUtils = null;

        static INativeUtils Utils
        {
            get
            {
                if(_nativeUtils == null)
                {
#if (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR
                    _nativeUtils = new IosNativeUtils();
#elif UNITY_ANDROID && !UNITY_EDITOR
                    _nativeUtils = new AndroidNativeUtils();
#else
                    _nativeUtils = new EmptyNativeUtils();
#endif
                }
                return _nativeUtils;
            }
        }

        static public bool IsInstalled(string appId)
        {
            return Utils.IsInstalled(appId);
        }
    
        static public void OpenApp(string appId)
        {
            Utils.OpenApp(appId);
        }
    
        static public void OpenStore(string appId)
        {
            Utils.OpenStore(appId);
        }
    
        static public void OpenUrl(string url)
        {
            Utils.OpenUrl(url);
        }

        static public bool UserAllowNotification
        {
            get
            {
                return Utils.UserAllowNotification;
            }
        }
    }
}
