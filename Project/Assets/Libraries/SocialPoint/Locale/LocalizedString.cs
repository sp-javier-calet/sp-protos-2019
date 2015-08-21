using UnityEngine;
using System.Collections;

namespace SocialPoint.Locale
{
    public class LocalizedString
    {
        private string _key;
        private string _defaultValue;
        private Localization _localization;

        public LocalizedString(string key, string defaultValue, Localization localization = null)
        {
            _key = key ?? string.Empty;
            _defaultValue = defaultValue;
            _localization = localization;
        }

        public override string ToString()
        {
            if(_localization == null)
            {
                _localization = Localization.Default;
            }

            string value = _localization.Get(_key);
            if(value.Length == 0)
            {
                if(_defaultValue.Length == 0)
                {
                    return _key;
                }

                return _defaultValue;
            }

            return value;
        }

        public static implicit operator string(LocalizedString locStr)
        {
            return locStr.ToString();
        }
    }
}
