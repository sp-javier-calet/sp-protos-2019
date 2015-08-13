using System;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    public class UnityAppEvents : AppEventsBase
    {
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
            OnWasOnBackground();
            OnOpenedFromSource(Source);
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
                OnOpenedFromSource(Source);
            }
            else
            {
                OnWillGoBackground();
                OnGoBackground();
            }
        }
    }
}

