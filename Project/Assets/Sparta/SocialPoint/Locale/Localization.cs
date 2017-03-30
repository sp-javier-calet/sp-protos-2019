using System.Collections.Generic;
using SocialPoint.Base;
using System.Text;
using SocialPoint.Utils;

namespace SocialPoint.Locale
{
    public sealed class Localization
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

        public Localization Fallback;

        readonly Dictionary<string,string> _strings = new Dictionary<string,string>();

        public Dictionary<string,string> Strings
        {
            get
            {
                return _strings;
            }
        }

        bool _showKeysOnDevMode;

        public bool ShowKeysOnDevMode
        {
            get
            {
                return _showKeysOnDevMode;
            }
            set
            {
                _showKeysOnDevMode = value && DebugUtils.IsDebugBuild;
            }
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
            if(!ShowKeysOnDevMode && Fallback != null && Fallback.ContainsKey(key))
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
            if(!ShowKeysOnDevMode && !string.IsNullOrEmpty(err.ClientMsg))
            {
                return err.ClientMsg;
            }
            return GetDefault(defaultString, err.ClientLocalize);
        }

        string GetDefault(string defaultString, string key = null)
        {
            if(ShowKeysOnDevMode && !string.IsNullOrEmpty(key))
            {
                StringBuilder stringBuilder = StringUtils.StartBuilder();

                stringBuilder.AppendFormat(DefaultFormat, Language, key);

                return StringUtils.FinishBuilder(stringBuilder);
            }
            return defaultString ?? key;
        }

        public string GetWithParams(string key, params object[] t)
        {
            return string.Format(Get(key), t);
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
            StringBuilder stringBuilder = StringUtils.StartBuilder();

            stringBuilder.Append(base.ToString()).AppendFormat(" ({0} elements)", _strings.Count);

            return StringUtils.FinishBuilder(stringBuilder);
        }
    }
}
