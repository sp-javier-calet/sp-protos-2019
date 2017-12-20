using System;
using System.Collections.Generic;
using System.Globalization;

namespace SocialPoint.Locale
{
    public interface ILocalizationManager : IDisposable
    {
        bool UseAlwaysDeviceLanguage{ get; set; }

        string[] SupportedLanguages{ get; set; }

        string CurrentLanguage{ get; set; }

        string SelectedLanguage{ get; }

        Localization Localization{ get; }

        CultureInfo SelectedCultureInfo { get; }

        event Action<Dictionary<string, Localization>> Loaded;
    }
}
