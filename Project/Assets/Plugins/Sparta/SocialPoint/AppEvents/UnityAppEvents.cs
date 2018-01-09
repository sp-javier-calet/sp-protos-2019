using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    public sealed class UnityAppEvents : BaseAppEvents
    {
        bool _openedFromSource;

        const string PlayerPrefSourceApplicationKey = "SourceApplicationKey";


        void Awake()
        {
            Source = new AppSource(new Dictionary<string, string> {
                { "type", "unity" }
            });
        }

        void Start()
        {
            var url = PlayerPrefs.GetString(PlayerPrefSourceApplicationKey);
            if(!string.IsNullOrEmpty(url))
            {
                LoadImpersonate(url);
                _openedFromSource = true;
                return;
            }

            if(!_openedFromSource)
            {
                OnOpenedFromSource(Source);
                _openedFromSource = true;
            }
        }

        void OnApplicationFocus(bool focusStatus)
        {
            if(focusStatus)
            {
                OnWasCovered();
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if(!pauseStatus)
            {
                OnWasOnBackground();
                if(!_openedFromSource)
                {
                    OnOpenedFromSource(Source);
                    _openedFromSource = true;
                }
            }
            else
            {
                OnWillGoBackground();
            }
        }

        public void LoadImpersonate(string url)
        {
            Source = new AppSource(url);
            OnOpenedFromSource(Source);
        }
    }
}

