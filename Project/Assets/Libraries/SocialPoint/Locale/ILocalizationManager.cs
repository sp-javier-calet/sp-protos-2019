using System;

namespace SocialPoint.Locale
{
    public interface ILocalizationManager
    {
        event Action Loaded;
        Localization Localization{ get; set; }
        void Load();        
    }
}
