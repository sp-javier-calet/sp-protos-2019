using System;
using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Dependency;
using SocialPoint.Locale;
using ObserverPattern;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPText")]
    public class SPText : Text, IObserver
    {
        public enum TextEffect
        {
            None,
            ToLower,
            ToUpper
        }

        public string Key;
        public string[] Parameters;
        public TextEffect Effect = TextEffect.None;

        string _text = string.Empty;
        ILocalizationManager _localizationManager;
        Localization _localization;

        protected override void Start()
        {
            base.Start();

            if(Application.isPlaying)
            {
                _localizationManager = Services.Instance.Resolve<ILocalizationManager>();
                if(_localizationManager == null)
                {
                    throw new InvalidOperationException("Could not find Localization");
                }

                _localization = _localizationManager.Localization;

                if(Services.Instance.Resolve("use_always_device_language", true))
                {
                    _localizationManager.AddObserver(this);
                }
                    
                LocalizeText();
            }
        }

        public void OnNotify()
        {
            LocalizeText();
        }

        void LocalizeText()
        {
            string localizedString;

            if(string.IsNullOrEmpty(Key))
            {
                if(!string.IsNullOrEmpty(_text))
                {
                    localizedString = _text;
                }
                else
                {
                    localizedString = base.text;
                }
            }
            else
            {
                if(Parameters != null && Parameters.Length > 0)
                {
                    localizedString = _localization.GetWithParams(Key, Parameters);
                }
                else
                {
                    localizedString = _localization.Get(Key);
                }
            }

            switch(Effect)
            {
            case TextEffect.ToLower:
                localizedString.ToLower(_localizationManager.SelectedCultureInfo);
                break;

            case TextEffect.ToUpper:
                localizedString.ToUpper(_localizationManager.SelectedCultureInfo);
                break;
            }

            text = localizedString;
        }

        public void SetKey(string key, TextEffect textEffect = TextEffect.None)
        {
            SetKey(key, null, textEffect);
        }

        public void SetKey(string key, string[] _parameters, TextEffect textEffect = TextEffect.None)
        {
            Effect = textEffect;
            Key = key;
            Parameters = _parameters;
            LocalizeText();
        }

        public void UpdateParameters(string[] _parameters)
        {
            Parameters = _parameters;
            LocalizeText();
        }

        public void Clear()
        {
            _text = string.Empty;
            Key = string.Empty;

            text = _text;
        }
    }
}