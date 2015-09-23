using SocialPoint.Locale;
using Zenject;

public class AdminPanelLocale : AdminPanelLocaleGUI
{
    [Inject]
    public LocalizationManager InjectLocalizationManager
    {
        set
        {
            LocalizationManager = value;
        }
    }

    [Inject]
    public Localization InjectLocalization
    {
        set
        {
            Localization = value;
        }
    }
}
