using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SocialPoint.AppEvents
{ 
    /// <summary>
    /// In order to receive events in Unity from Ios we use UnitySendMessage("IosAppEvents","NotifyStatus","ACTIVE")
    /// we need to create a persistant gameObject Containig this script
    /// </summary>
    public class IosAppEvents : AppEventsBase
    {
        private enum Status
        {
            FIRSTBOOT,
            ACTIVE,
            BACKGROUND,
            WILLGOBACKGROUND,
            WILLGOFOREGROUND,
            MEMORYWARNING,
            UPDATEDSOURCE
        }

        private const string PlayerPrefSourceApplicationKey = "SourceApplicationKey";

        private List<Status> EventStatus = new List<Status> {Status.MEMORYWARNING, Status.UPDATEDSOURCE};
        private Status _previousStatus = Status.FIRSTBOOT;

        [DllImport ("__Internal")]
        private static extern void SPUnityAppEvents_Init(string name);

        [DllImport ("__Internal")]
        private static extern void SPUnityAppEvents_Flush();

        void Awake()
        {
            Source = new AppSource();
            // Set the GameObject name to the class name for easy access from native plugin
            gameObject.name = GetType().ToString();
            DontDestroyOnLoad(this);
            SPUnityAppEvents_Init(gameObject.name);
        }

        private void Start()
        {
            /* Early native events can't be sent to the Unity counterpart, 
             * so they must be flushed manually */
            SPUnityAppEvents_Flush();
        }

        private void UpdateStatus(Status status)
        {
            /* Store only real statuses (background/foreground/active..). 
             * Other events could break the checks for WasCovered and WasOnBackground */
            if(!EventStatus.Contains(status))
            {
                _previousStatus = status;
            }
        }

        private void CheckApplicationSource()
        {
            OnOpenedFromSource(Source); 
        }

        private void ClearAppSource()
        {
            Source = new AppSource();
        }
        
        void NotifyStatus(string message)
        {
            Status status = (Status)Enum.Parse(typeof(Status), message.ToUpper());

            switch(status)
            {
            case Status.ACTIVE:
                /* On Active events depends on the previous status */
                switch(_previousStatus)
                {
                case Status.WILLGOBACKGROUND:
                    // App was partially hidden
                    OnWasCovered();
                    break;

                case Status.WILLGOFOREGROUND:
                    // App was stopped in background
                    OnWasOnBackground();
                    CheckApplicationSource();
                    break;

                case Status.FIRSTBOOT:
                    CheckApplicationSource();
                    break;
                }
                break;
                
            case Status.WILLGOBACKGROUND:
                OnWillGoBackground();
                OnGoBackground();
                break;

            case Status.BACKGROUND:
                break;

            case Status.WILLGOFOREGROUND:
                // Clear current app source. Any new source will be notified after this event.
                ClearAppSource();
                break;
                
            case Status.MEMORYWARNING:
                OnReceivedMemoryWarning();
                break;

            case Status.UPDATEDSOURCE:
                string sourceUri = PlayerPrefs.GetString(PlayerPrefSourceApplicationKey);
                Source = new AppSource(sourceUri);
                break;
            }

            UpdateStatus(status);
        }
    }
}

