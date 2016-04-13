using System;
using System.Collections;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    /// <summary>
    /// Manage interface events and provides a common base implementation for all platform-dependent classes
    /// </summary>
    public abstract class BaseAppEvents : MonoBehaviour, IAppEvents, ICoroutineRunner
    {
        public PriorityAction GameWasLoaded { get; private set; }

        public PriorityCoroutineAction AfterGameWasLoaded{ get; }

        public PriorityAction GameWillRestart { get; private set; }

        public PriorityAction WillGoBackground { get; private set; }

        protected BaseAppEvents()
        {
            GameWasLoaded = new PriorityAction();
            AfterGameWasLoaded = new PriorityCoroutineAction(this);
            GameWillRestart = new PriorityAction();
            WillGoBackground = new PriorityAction();
        }

        public void Dispose()
        {
            GameWasLoaded.Clear();
            AfterGameWasLoaded.Clear();
            GameWillRestart.Clear();
            WillGoBackground.Clear();
            WasOnBackground = null;
            WasCovered = null;
            ReceivedMemoryWarning = null;
            OpenedFromSource = null;
        }

        public void TriggerGameWasLoaded()
        {
            OnGameWasLoaded();
        }

        protected void OnGameWasLoaded()
        {
            GameWasLoaded.Run();
            AfterGameWasLoaded.Run();
        }

        public void TriggerGameWillRestart()
        {
            OnGameWillRestart();
        }

        protected void OnGameWillRestart()
        {
            GameWillRestart.Run();
        }

        public void TriggerMemoryWarning()
        {
            OnReceivedMemoryWarning();
        }

        public void TriggerWillGoBackground()
        {
            OnWillGoBackground();
        }

        protected  void OnWillGoBackground()
        {
            WillGoBackground.Run();
        }

        public void TriggerApplicationQuit()
        {
            OnApplicationQuit();
        }

        public event Action WasOnBackground;

        protected void OnWasOnBackground()
        {
            var handler = WasOnBackground;
            if(handler != null)
            {
                handler();
            }
        }

        public event Action WasCovered;

        protected void OnWasCovered()
        {
            var handler = WasCovered;
            if(handler != null)
            {
                handler();
            }
        }

        public event Action ReceivedMemoryWarning;

        protected void OnReceivedMemoryWarning()
        {
            var handler = ReceivedMemoryWarning;
            if(handler != null)
            {
                handler();
            }
        }

        public event Action<AppSource> OpenedFromSource;

        protected void OnOpenedFromSource(AppSource source)
        {
            var handler = OpenedFromSource;
            if(handler != null)
            {
                handler(source);
            }
        }

        public AppSource Source { get; protected set; }

        public event Action ApplicationQuit;

        void OnApplicationQuit()
        {
            var handler = ApplicationQuit;
            if(handler != null)
            {
                handler();
            }
        }

        public event Action<int> LevelWasLoaded;

        void OnLevelWasLoaded(int level)
        {
            var handler = LevelWasLoaded;
            if(handler != null)
            {
                handler(level);
            }
        }

        IEnumerator ICoroutineRunner.StartCoroutine(IEnumerator enumerator)
        {
            if(enumerator != null)
            {
                StartCoroutine(enumerator);
            }
            return enumerator;
        }
    }
}
