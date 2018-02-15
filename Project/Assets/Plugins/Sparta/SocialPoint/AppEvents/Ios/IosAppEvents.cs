#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define IOS_TVOS_DEVICE
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace SocialPoint.AppEvents
{
    /// <summary>
    /// In order to receive events in Unity from Ios we use UnitySendMessage("IosAppEvents","NotifyStatus","ACTIVE")
    /// we need to create a persistant gameObject Containig this script
    /// </summary>
    public sealed class IosAppEvents : BaseAppEvents
    {
        enum Status
        {
            FIRSTBOOT,
            ACTIVE,
            BACKGROUND,
            WILLGOBACKGROUND,
            WILLGOFOREGROUND,
            MEMORYWARNING,
            UPDATEDSOURCE
        }

        const string PlayerPrefSourceApplicationKey = "SourceApplicationKey";

        List<Status> EventStatus = new List<Status> { Status.MEMORYWARNING, Status.UPDATEDSOURCE };
        Status _previousStatus = Status.FIRSTBOOT;

#if IOS_TVOS_DEVICE
        [DllImport ("__Internal")]
        private static extern void SPUnityAppEvents_Init(string name);

        [DllImport ("__Internal")]
        private static extern void SPUnityAppEvents_Flush();
#else
        static  void SPUnityAppEvents_Init(string name)
        {
        }

        static void SPUnityAppEvents_Flush()
        {
        }
#endif

        void Awake()
        {
            Source = new AppSource();
            // Set the GameObject name to the class name for easy access from native plugin
            gameObject.name = GetType().ToString();
            if(gameObject.transform.parent == null)
            {
                DontDestroyOnLoad(this);
            }
            SPUnityAppEvents_Init(gameObject.name);
        }

        void Start()
        {
            /* Early native events can't be sent to the Unity counterpart, 
             * so they must be flushed manually */
            SPUnityAppEvents_Flush();
        }

        void UpdateStatus(Status status)
        {
            /* Store only real statuses (background/foreground/active..). 
             * Other events could break the checks for WasCovered and WasOnBackground */
            if(!EventStatus.Contains(status))
            {
                _previousStatus = status;
            }
        }

        void CheckApplicationSource()
        {
            OnOpenedFromSource(Source); 
        }

        void ClearAppSource()
        {
            Source = new AppSource();
        }

        void NotifyStatus(string message)
        {
            var status = (Status)Enum.Parse(typeof(Status), message.ToUpper());

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

