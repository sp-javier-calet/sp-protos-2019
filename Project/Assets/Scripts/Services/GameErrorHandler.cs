
using SocialPoint.Hardware;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

class GameErrorHandler : SocialPoint.GameLoading.GameErrorHandler
{
    public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents):
        base(alert, locale, appEvents, FindPopups)
    {
        Debug = ServiceLocator.Instance.Resolve<bool>("game_debug");
    }

    static UIStackController FindPopups()
    {
        return GameObject.FindObjectOfType<PopupsController>();
    }
}
