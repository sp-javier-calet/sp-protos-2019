
using SocialPoint.Hardware;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.GUIControl;
using UnityEngine;
using Zenject;

class GameErrorHandler : SocialPoint.GameLoading.GameErrorHandler
{
    public GameErrorHandler(IAlertView alert, Localization locale, IAppEvents appEvents):
        base(alert, locale, appEvents, FindPopups)
    {
    }

    [Inject("game_debug")]
    bool injectDebug
    {
        set
        {
            Debug = value;
        }
    }

    static UIStackController FindPopups()
    {
        return GameObject.FindObjectOfType<PopupsController>();
    }
}
