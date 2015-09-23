using System;

namespace SocialPoint.Locale
{
    public interface ILocalizationManager
    {
        string[] SupportedLanguages{ get; set; }
        string CurrentLanguage{ get; set; }
        Localization Localization{ get; set; }

        event Action Loaded;

        void Load();
    }
}
