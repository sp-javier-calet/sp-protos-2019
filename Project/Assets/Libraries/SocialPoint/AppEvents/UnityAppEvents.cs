using System;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    public class UnityAppEvents : BaseAppEvents
    {
        bool _openedFromSource = false;

        private void Awake()
        {
            // Set the GameObject name to the class name for easy access from native plugin
            gameObject.name = GetType().ToString();
            DontDestroyOnLoad(this);

            Source = new AppSource(new Dictionary<string, string>{
                {"type", "unity"}
            });
        }

        private void Start()
        {
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
    }
}

