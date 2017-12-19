using System;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    
    public sealed class AndroidAppEvents : BaseAppEvents
    {
        void Awake()
        {
            // Register application listeners
            AndroidContext.CurrentApplication.Call("registerActivityLifecycleCallbacks", new ApplicationLifecycleDelegate(this));
            AndroidContext.CurrentActivity.Call("registerComponentCallbacks", new MemoryTrimListener(this));
        }

        void Start()
        {
            DispatchMainThread(StartDispatched);
        }

        void StartDispatched()
        {
            // Trigger OpenFromSource at startup
            UpdateSource();
            OnOpenedFromSource(Source);
        }

        void UpdateSource()
        {
            string sourceUri = AndroidContext.CurrentActivity.Call<string>("collectApplicationSource");
            Source = new AppSource(sourceUri);
        }

        
        #region Events dispatch

        Action _dispatched;

        void LateUpdate()
        {
            DispatchPending();
        }

        void OnApplicationPause(bool pause)
        {
            /* Force dispatch when application is paused,
             * since system pause events could arrive after LateUpdate, 
             * and they would be processed when the app is back to foreground */
            DispatchPending();
        }

        void DispatchPending()
        {
            if(_dispatched != null)
            {
                var dispatched = _dispatched;
                _dispatched = null;
                dispatched();
            }
        }

        void DispatchMainThread(Action action)
        {
            /* System events have to be dispatched to the current Unity Main Thread 
             * since this changes between Development and Production builds. */
            _dispatched += action;
        }

        public void OnActivityResumed(bool stopped)
        {
            DispatchMainThread(() => OnActivityResumedDispatched(stopped));
        }

        void OnActivityResumedDispatched(bool stopped)
        {
            if(stopped)
            {
                OnWasOnBackground();

                /* After come from background, check the application source and send source event.
                 * Android platform retrieve the application source on demand */
                UpdateSource();
                OnOpenedFromSource(Source);
            }
            else
            {
                /* If Application was paused but not stopped, 
                 * it means that it was covered by another app in front
                 */
                OnWasCovered();
            }
        }

        public void OnActivityPaused()
        {
            DispatchMainThread(OnActivityPausedDispatched);
        }

        void OnActivityPausedDispatched()
        {
            OnWillGoBackground();
        }

        public void OnMemoryWarning()
        {
            DispatchMainThread(OnReceivedMemoryWarning);
        }

        
        #endregion

        
        #region Java proxies

        class ApplicationLifecycleDelegate : AndroidJavaProxy
        {
            const string JavaInterface = "android.app.Application$ActivityLifecycleCallbacks";
            AndroidAppEvents _appEvents;
            bool WasStopped;

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
                _appEvents.OnActivityPaused();
            }

            public void onActivityResumed(AndroidJavaObject activity)
            {
                _appEvents.OnActivityResumed(WasStopped);
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

        class MemoryTrimListener : AndroidJavaProxy
        {
            const string JavaInterface = "android.content.ComponentCallbacks2";
            AndroidAppEvents _appEvents;
            int LevelMemoryComplete;

            public MemoryTrimListener(AndroidAppEvents appEvents)
                : base(JavaInterface)
            {
                _appEvents = appEvents;
                using(var javaClass = new AndroidJavaClass(JavaInterface))
                {
                    LevelMemoryComplete = javaClass.GetStatic<int>("TRIM_MEMORY_COMPLETE");
                }
            }

            public void onConfigurationChanged(AndroidJavaObject newConfig)
            {
            }

            public void onTrimMemory(int level)
            {                
                if(level == LevelMemoryComplete)
                {
                    _appEvents.OnMemoryWarning();
                }
            }
        }


        
        #endregion
    
    }

    
    #else
    public sealed class AndroidAppEvents : BaseAppEvents
    {
    }
    #endif
}
