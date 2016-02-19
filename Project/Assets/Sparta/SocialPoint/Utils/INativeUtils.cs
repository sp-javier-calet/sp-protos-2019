namespace SocialPoint.Utils
{
    public interface INativeUtils
    {
        bool IsInstalled(string appId);

        void OpenApp(string appId);

        void OpenStore(string appId);

        void OpenUrl(string url);
    }
}