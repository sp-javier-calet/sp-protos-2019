using UnityEngine;
using UnityEngine.UI;
using SocialPoint.Dependency;
using SocialPoint.Locale;
using System;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPText")]
    public class SPText : Text
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

                if(!_localizationManager.UseAlwaysDeviceLanguage)
                {
                    _localizationManager.Loaded += OnChangeLanguage;
                }
                    
                LocalizeText();
            }
        }

        protected override void OnDestroy()
        {
            if(Application.isPlaying)
            {
                if(_localizationManager != null && !_localizationManager.UseAlwaysDeviceLanguage)
                {
                    _localizationManager.Loaded -= OnChangeLanguage;
                }
            }

            base.OnDestroy();
        }

        void OnChangeLanguage(Dictionary<string, Localization> loaded)
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

        public void Refresh()
        {
            LocalizeText();
        }
    }
}