using System;
using System.Collections.Generic;

namespace SocialPoint.Locale
{
    public interface ILocalizationManager : IDisposable
    {
        string[] SupportedLanguages{ get; set; }

        string CurrentLanguage{ get; set; }

        string SelectedLanguage{ get; }

        Localization Localization{ get; set; }

        event Action<Dictionary<string, Localization>> Loaded;

        void Load();
    }
}
