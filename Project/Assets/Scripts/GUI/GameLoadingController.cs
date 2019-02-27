using SocialPoint.GameLoading;

public class GameLoadingController : LoadingController
{
    LoadingOperation gameOperation;

    protected override void OnLoad()
    {
        base.OnLoad();

        gameOperation = new LoadingOperation(0.0f);

        LoadingManager.RegisterOperation(gameOperation);
    }

    public override bool OnBeforeClose()
    {
        return false;
    }
}
