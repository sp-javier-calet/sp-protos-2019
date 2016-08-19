using UnityEngine;
using UnityEngine.UI;
using System;
using SocialPoint.Locale;

namespace SocialPoint.GameLoading
{
    public sealed class GameLoadingBarController : MonoBehaviour
    {
        [SerializeField]
        Slider _slider;

        [SerializeField]
        Text _log;

        public string Message
        {
            get
            {
                if(_log != null)
                {
                    return _log.text;
                }
                return null;
            }

            set
            {
                if(_log != null)
                {
                    _log.text = value;
                }
            }
        }

        public float Percent
        {
            get
            {
                if(_slider != null)
                {
                    return _slider.normalizedValue;
                }
                return 0.0f;
            }

            set
            {
                if(_slider != null)
                {
                    _slider.normalizedValue = value;
                }
            }
        }            
    }
}