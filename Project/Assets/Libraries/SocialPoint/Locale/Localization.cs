using System.Collections.Generic;
using System.Text;
using SocialPoint.IO;

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

        /* 
         * Static default instance
         */
        private static Localization _defaultLocalization = new Localization();
        public static Localization Default
        { 
            get
            {
                return _defaultLocalization;
            } 
        }

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

        const string DefaultFormat = "<{0}>";

        public string Get(string key, string defaultString=null)
        {
            string value;
            if(!_strings.TryGetValue(key, out value))
            {
                value = defaultString;
            }
            if(value == null)
            {
                value = string.Format(DefaultFormat, key);
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
            return base.ToString()+string.Format(" ({0} elements)", _strings.Count);
        }
    }
}
