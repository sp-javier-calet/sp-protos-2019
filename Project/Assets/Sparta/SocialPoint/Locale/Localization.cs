using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Locale
{
    public class Localization
    {
        public const string EnglishIdentifier = "en";
        public const string SpanishIdentifier = "es";
        public const string GermanIdentifier = "de";
        public const string FrenchIdentifier = "fr";
        public const string TurkishIdentifier = "tr";
        public const string ItalianIdentifier = "it";
        public const string PortugueseIdentifier = "pt";
        public const string RussianIdentifier = "ru";
        public const string JapaneseIdentifier = "ja";
        public const string KoreanIdentifier = "ko";
        public const string BrasilianIdentifier = "br";
        public const string GalicianIdentifier = "gl";
        public const string BasqueIdentifier = "eu";
        public const string CatalanIdentifier = "ca";
        public const string SimplifiedChineseIdentifier = "zh-Hans";
        public const string TraditionalChineseIdentifier = "zh-Hant";
        public const string TraditionalHongKongChineseIdentifier = "zh";

        // Android can not return languageCode + script. So we need to disambiguate using the country code instead.
        public const string SimplifiedChineseIdentifierCountry = "zh-CN";
        public const string TraditionalChineseIdentifierCountry = "zh-TW";
        public const string TraditionalHongKongChineseIdentifierCountry = "zh-HK";

        static Localization _defaultLocalization;

        public static Localization Default
        {
            get
            {
                if(_defaultLocalization == null)
                {
                    _defaultLocalization = new Localization();
                }
                return _defaultLocalization;
            }
        }

        public Localization Fallback;

        readonly Dictionary<string,string> _strings = new Dictionary<string,string>();

        public Dictionary<string,string> Strings
        {
            get
            {
                return _strings;
            }
        }

        public bool Debug;

        public Localization()
        {
            Debug = UnityEngine.Debug.isDebugBuild;
        }

        string _language = "";

        public string Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        const string DefaultFormat = "{0}<{1}>";

        public string Get(string key, string defaultString = null)
        {
            string value;
            if(_strings.TryGetValue(key, out value))
            {
                return value;
            }
            if(Fallback != null && Fallback.ContainsKey(key))
            {
                return Fallback.Get(key);
            }
            return GetDefault(defaultString, key);
        }

        public string Get(Error err, string defaultString = null)
        {
            if(!string.IsNullOrEmpty(err.ClientLocalize) && ContainsKey(err.ClientLocalize))
            {
                return Get(err.ClientLocalize, err.ClientMsg);
            }
            if(!Debug && !string.IsNullOrEmpty(err.ClientMsg))
            {
                return err.ClientMsg;
            }
            return GetDefault(defaultString, err.ClientLocalize);
        }

        string GetDefault(string defaultString, string key = null)
        {
            if(Debug && !string.IsNullOrEmpty(key))
            {
                return string.Format(DefaultFormat, Language, key);
            }
            return defaultString;
        }

        public void Clear()
        {
            _strings.Clear();
        }

        public bool ContainsKey(string key)
        {
            return _strings.ContainsKey(key);
        }

        public void Set(string key, string value)
        {
            _strings[key] = value;
        }

        public void Add(Localization other)
        {
            var itr = other._strings.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                Set(pair.Key, pair.Value);
            }
            itr.Dispose();
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" ({0} elements)", _strings.Count);
        }
    }
}
