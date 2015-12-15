using SocialPoint.Utils;

namespace SocialPoint.Utils
{
    public class EmptyNativeUtils : INativeUtils
    {
        public bool IsInstalled(string appId)
        {
            return false;
        }
        
        public void OpenApp(string appId)
        {
        }
        
        public void OpenStore(string appId)
        {
        }

        public void OpenUrl(string url)
        {
        }
    }
}