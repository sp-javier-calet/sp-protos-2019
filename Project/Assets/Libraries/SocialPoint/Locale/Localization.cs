using System.Collections.Generic;

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
        public const string ChineseIdentifier = "zh";

        private static Localization _defaultLocalization = null;

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

        private Dictionary<string,string> _strings = new Dictionary<string,string>();

        public Dictionary<string,string> Strings
        {
            get
            {
                return _strings;
            }
        }

        public Localization()
        {
        }

        private string _language = "";

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

        string DefaultFormat = "{0}<{1}>";

        public string Get(string key, string defaultString = null)
        {
            string value = string.Empty;
            if(!_strings.TryGetValue(key, out value))
            {
                if(Fallback != null && Fallback.ContainsKey(key))
                {
                    value = Fallback.Get(key);
                }
                else
                {
                    value = string.Format(DefaultFormat, Language, key);
                }
            }
            return value;
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
            foreach(var pair in other._strings)
            {
                Set(pair.Key, pair.Value);
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format(" ({0} elements)", _strings.Count);
        }
    }
}
