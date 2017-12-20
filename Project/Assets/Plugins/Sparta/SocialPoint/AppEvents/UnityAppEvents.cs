using System.Collections.Generic;

namespace SocialPoint.AppEvents
{
    public sealed class UnityAppEvents : BaseAppEvents
    {
        bool _openedFromSource;

        void Awake()
        {
            Source = new AppSource(new Dictionary<string, string> {
                { "type", "unity" }
            });
        }

        void Start()
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

