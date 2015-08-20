using UnityEngine;
using System;
using System.Collections;
using SocialPoint.Base;
using SocialPoint.Threading;

namespace SocialPoint.AppEvents
{
#if UNITY_ANDROID

	public class AndroidAppEvents : AppEventsBase
    {
        void Awake()
        {
            // Register application listeners
            AndroidContext.CurrentApplication.Call("registerActivityLifecycleCallbacks", new ApplicationLifecycleDelegate(this));
            AndroidContext.CurrentActivity.Call("registerComponentCallbacks", new MemoryTrimListener(this));
        }

        void Start()
        {
            // Trigger OpenFromSource at startup
            UpdateSource();
            OnOpenedFromSource(Source);
        }

        private void UpdateSource()
        {
            string sourceUri = AndroidContext.CurrentActivity.Call<string>("collectApplicationSource");
            Source = new AppSource(sourceUri);
        }
        
        #region Events dispatch

        private void DispatchWasCovered()
        {
            OnWasCovered();
        }

        private void DispatchWasOnBackground()
        {
            OnWasOnBackground();

            /* After come from background, check the application source and send source event.
             * Android platform retrieve the application source on demand */
            UpdateSource();
            OnOpenedFromSource(Source);
        }
        
        private void DispatchWillGoBackground()
        {
            OnWillGoBackground();
            OnGoBackground();
        }

        private void DispatchMemoryWarning()
        {
            OnReceivedMemoryWarning();
        }

    #endregion

    #region Java proxies
        private class ApplicationLifecycleDelegate : AndroidJavaProxy
        {
            private const string JavaInterface = "android.app.Application$ActivityLifecycleCallbacks";
            private AndroidAppEvents _appEvents;
            private bool WasStopped = false;

            public ApplicationLifecycleDelegate(AndroidAppEvents appEvents)
                : base(JavaInterface)
            {
                _appEvents = appEvents;
            }

            public void onActivityCreated(AndroidJavaObject activity, AndroidJavaObject savedInstanceState) 
            {
            }

            public void onActivityDestroyed(AndroidJavaObject activity) 
            {
            }

            public void onActivityPaused(AndroidJavaObject activity) 
            {
                MainThreadDispatcher.Dispatch(_appEvents.DispatchWillGoBackground);
            }

            public void onActivityResumed(AndroidJavaObject activity) 
            {
                /* If Application was paused but not stopped, 
                 * it means that it was covered by another app in front
                 */
                if(WasStopped)
                {
                    MainThreadDispatcher.Dispatch(_appEvents.DispatchWasOnBackground);
                }
                else
                {
                    MainThreadDispatcher.Dispatch(_appEvents.DispatchWasCovered);
                }

                WasStopped = false;
            }

            public void onActivitySaveInstanceState(AndroidJavaObject activity, AndroidJavaObject outState) 
            {
            }

            public void onActivityStarted(AndroidJavaObject activity) 
            {
            }

            public void onActivityStopped(AndroidJavaObject activity) 
            {
                WasStopped = true;
            }
        }

        private class MemoryTrimListener : AndroidJavaProxy
        {
            private const string JavaInterface = "android.content.ComponentCallbacks2";
            private AndroidAppEvents _appEvents;
            private int LevelMemoryComplete;

            public MemoryTrimListener(AndroidAppEvents appEvents)
                : base(JavaInterface)
            {
                _appEvents = appEvents;
                LevelMemoryComplete = new AndroidJavaClass(JavaInterface).GetStatic<int>("TRIM_MEMORY_COMPLETE");
            }

            public void onConfigurationChanged(AndroidJavaObject newConfig) 
            {
            }
            
            public void onTrimMemory(int level) 
            {                
                if(level == LevelMemoryComplete)
                {
                    MainThreadDispatcher.Dispatch(_appEvents.DispatchMemoryWarning);
                }
            }
        }


    #endregion

	}

#else
	public class AndroidNetworkInfo : AppEventsBase
	{
	}
#endif
}
