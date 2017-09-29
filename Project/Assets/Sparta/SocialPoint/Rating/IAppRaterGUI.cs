
namespace SocialPoint.Rating
{
    public interface IAppRaterGUI
    {
        bool Show(bool showLaterButton);

        void Rate();

        IAppRater AppRater{ set; }
    }
}