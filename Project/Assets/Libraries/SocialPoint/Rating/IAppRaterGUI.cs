

namespace SocialPoint.Rating
{
    public interface IAppRaterGUI
    {
        void Show(bool showLaterButton);
        void SetAppRater(IAppRater appRater);
    }
}