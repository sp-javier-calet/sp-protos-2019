using System;
using System.Collections.Generic;
using System.Globalization;
using ObserverPattern;

namespace SocialPoint.Locale
{
    public interface ILocalizationManager : IObservable, IDisposable
    {
        string[] SupportedLanguages{ get; set; }

        string CurrentLanguage{ get; set; }

        string SelectedLanguage{ get; }

        Localization Localization{ get; }

        CultureInfo SelectedCultureInfo { get; }

        event Action<Dictionary<string, Localization>> Loaded;
    }
}
