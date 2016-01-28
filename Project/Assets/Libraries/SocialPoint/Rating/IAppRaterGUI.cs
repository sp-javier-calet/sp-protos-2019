

namespace SocialPoint.Rating
{
    public interface IAppRaterGUI
    {
        bool Show(bool showLaterButton);
        void SetAppRater(IAppRater appRater);
    }
}
