using UnityEngine;
using System;

namespace SocialPoint.Hardware
{
    public sealed class UnityAppInfo : IAppInfo
    {
        string _seedId;

        public string SeedId
        {
            get
            {
                return _seedId;
            }

            set
            {
                _seedId = value;
            }
        }

        string _id;

        public string Id
        {
            get
            {
#if UNITY_EDITOR
                if(_id == null)
                {
                #if UNITY_2017
                    _id = UnityEditor.PlayerSettings.applicationIdentifier;
                #else
                    _id = UnityEditor.PlayerSettings.bundleIdentifier;
                #endif
                }
#endif
                return _id;
            }

            set
            {
                _id = value;
            }
        }

        string _version;

        public string Version
        {
            get
            {
                return _version ?? DateTime.UtcNow.ToString("yyMMddHHmm");
            }
            set
            {
                _version = value;
            }
        }

        string _shortVersion;

        public string ShortVersion
        {
            get
            {
#if UNITY_EDITOR
                if(_shortVersion == null)
                {
                    _shortVersion = UnityEditor.PlayerSettings.bundleVersion;
                }
#endif
                return _shortVersion;
            }

            set
            {
                _shortVersion = value;
            }
        }

        string _language;

        public string Language
        {
            get
            {
                if(_language == null)
                {
                    switch(Application.systemLanguage)
                    {
                    case SystemLanguage.Afrikaans:
                        _language = "af";
                        break;
                    case SystemLanguage.Arabic:
                        _language = "ar";
                        break;
                    case SystemLanguage.Basque:
                        _language = "eu";
                        break;
                    case SystemLanguage.Belarusian:
                        _language = "be";
                        break;
                    case SystemLanguage.Bulgarian:
                        _language = "bg";
                        break;
                    case SystemLanguage.Catalan:
                        _language = "ca";
                        break;
                    case SystemLanguage.Chinese:
                        _language = "zh";
                        break;
                    case SystemLanguage.Czech:
                        _language = "cs";
                        break;
                    case SystemLanguage.Danish:
                        _language = "da";
                        break;
                    case SystemLanguage.Dutch:
                        _language = "nl";
                        break;
                    case SystemLanguage.English:
                        _language = "en";
                        break;
                    case SystemLanguage.Estonian:
                        _language = "et";
                        break;
                    case SystemLanguage.Faroese:
                        _language = "fo";
                        break;
                    case SystemLanguage.Finnish:
                        _language = "fi";
                        break;
                    case SystemLanguage.French:
                        _language = "fr";
                        break;
                    case SystemLanguage.German:
                        _language = "de";
                        break;
                    case SystemLanguage.Greek:
                        _language = "el";
                        break;
                    case SystemLanguage.Hebrew:
                        _language = "he";
                        break;
                    case SystemLanguage.Icelandic:
                        _language = "is";
                        break;
                    case SystemLanguage.Indonesian:
                        _language = "id";
                        break;
                    case SystemLanguage.Italian:
                        _language = "it";
                        break;
                    case SystemLanguage.Japanese:
                        _language = "ja";
                        break;
                    case SystemLanguage.Korean:
                        _language = "ko";
                        break;
                    case SystemLanguage.Latvian:
                        _language = "lv";
                        break;
                    case SystemLanguage.Lithuanian:
                        _language = "lo";
                        break;
                    case SystemLanguage.Norwegian:
                        _language = "no";
                        break;
                    case SystemLanguage.Polish:
                        _language = "pl";
                        break;
                    case SystemLanguage.Portuguese:
                        _language = "pt";
                        break;
                    case SystemLanguage.Romanian:
                        _language = "ro";
                        break;
                    case SystemLanguage.Russian:
                        _language = "ru";
                        break;
                    case SystemLanguage.SerboCroatian:
                        _language = "sr";
                        break;
                    case SystemLanguage.Slovak:
                        _language = "sk";
                        break;
                    case SystemLanguage.Slovenian:
                        _language = "sl";
                        break;
                    case SystemLanguage.Spanish:
                        _language = "es";
                        break;
                    case SystemLanguage.Swedish:
                        _language = "sv";
                        break;
                    case SystemLanguage.Thai:
                        _language = "th";
                        break;
                    case SystemLanguage.Turkish:
                        _language = "tr";
                        break;
                    case SystemLanguage.Ukrainian:
                        _language = "uk";
                        break;
                    case SystemLanguage.Vietnamese:
                        _language = "vi";
                        break;
                    case SystemLanguage.Hungarian:
                        _language = "hu";
                        break;
                    default:
                        _language = string.Empty;
                        break;
                    }
                }
                return _language;
            }

            set
            {
                _language = value;
            }
        }

        string _country;

        public string Country
        {
            get
            {
                return _country;
            }

            set
            {
                _country = value;
            }
        }

        override public string ToString()
        {
            return InfoToStringExtension.ToString(this);
        }
    }
}

