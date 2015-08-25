using UnityEngine;
using SocialPoint.Login;
using SocialPoint.Locale;
using Zenject;

public class GameLoadingInstaller : MonoInstaller
{
    [Inject]
    ILogin Login;

    [Inject]
    PopupsController Popups;

    [Inject]
    Localization Localization;

    public override void InstallBindings()
    {
        Container.BindInstance("loading_controller_login", Login);
        Container.BindInstance("loading_controller_Popup", Popups);
        Container.BindInstance("loading_controller_Localization", Localization);
    }
}
