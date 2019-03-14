using SocialPoint.GameLoading;

public class GameLoadingController : LoadingController
{
    public override bool OnBeforeClose()
    {
        return false;
    }
}
