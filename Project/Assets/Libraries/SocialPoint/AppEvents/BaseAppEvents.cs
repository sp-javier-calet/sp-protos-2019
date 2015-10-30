using System;
using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.AppEvents
{
    /// <summary>
    /// Manage interface events and provides a common base implementation for all platform-dependent classes
    /// </summary>
    public abstract class BaseAppEvents : MonoBehaviour, IAppEvents 
    {
        #region IAppEvents implementation

        public void Dispose()
        {
            WasOnBackground = null;
            WasCovered = null;
            ReceivedMemoryWarning = null;
            OpenedFromSource = null;
        }

        #region Game Events

        public void TriggerGameWasLoaded()
        {
            OnGameWasLoaded();
        }
                
        PriorityAction _gameWasLoaded = new PriorityAction();
        
        public void RegisterGameWasLoaded(int priority, Action action)
        {
            _gameWasLoaded.Add(priority, action);
        }
        
        public void UnregisterGameWasLoaded(Action action)
        {
            _gameWasLoaded.Remove(action);
        }
                
        public void RestartGame()
        {
            OnGameWillRestart();
            Application.LoadLevel(0);
        }
        
        PriorityAction _gameWillRestart = new PriorityAction();
        
        public void RegisterGameWillRestart(int priority, Action action)
        {
            _gameWillRestart.Add(priority, action);
        }
        
        public void UnregisterGameWillRestart(Action action)
        {
            _gameWillRestart.Remove(action);
        }
                
        /// <summary>
        /// Occurs after the game is loaded.
        /// </summary>
        protected  void OnGameWasLoaded()
        {
            _gameWasLoaded.Run();
        }
        
        /// <summary>
        /// Occurs before game is restarted
        /// </summary>
        protected  void OnGameWillRestart()
        {
            _gameWillRestart.Run();
        }

        #endregion

        #region Native Events
        
        public void TriggerMemoryWarning()
        {
            OnReceivedMemoryWarning();
        }
        
        public void TriggerWillGoBackground()
        {
            OnWillGoBackground();
        }

        PriorityAction _willGoBackground = new PriorityAction();

        public void RegisterWillGoBackground(int priority, Action action)
        {
            _willGoBackground.Add(priority, action);
        }

        public void UnregisterWillGoBackground(Action action)
        {
            _willGoBackground.Remove(action);
        }

        /// <summary>
        /// Occurs before going to background.
        /// </summary>
        protected  void OnWillGoBackground()
        {
            _willGoBackground.Run();
        }
        
        /// <summary>
        /// Occurs when was on background.
        /// </summary>
        public event Action WasOnBackground;
        
        protected void OnWasOnBackground()
        {
            var handler = WasOnBackground;
            if(handler != null)
            {
                handler();
            }
        }
        
        /// <summary>
        /// Occurs when was covered.
        /// </summary>
        public event Action WasCovered;
        
        protected void OnWasCovered()
        {
            var handler = WasCovered;
            if(handler != null)
            {
                handler();
            }
        }
        
        /// <summary>
        /// Occurs when recieved memory warning.
        /// </summary>
        public event Action ReceivedMemoryWarning;
        
        protected void OnReceivedMemoryWarning()
        {
            var handler = ReceivedMemoryWarning;
            if(handler != null)
            {
                handler();
            }
        }

        /// <summary>
        /// Occurs when app is opened with source data
        /// </summary>
        public event Action<AppSource> OpenedFromSource;
        
        protected void OnOpenedFromSource(AppSource source)
        {
            var handler = OpenedFromSource;
            if(handler != null)
            {
                handler(source);
            }
        }

        public AppSource Source { get; protected set;}

        #endregion

        #region Unity Events
        
        /// <summary>
        /// Occurs when application quits.
        /// </summary>
        public event Action ApplicationQuit;
        
        void OnApplicationQuit()
        {
            var handler = ApplicationQuit;
            if(handler != null)
            {
                handler();
            }
        }
        
        /// <summary>
        /// Occurs when level is loaded non additive.
        /// </summary>
        public event Action<int> LevelWasLoaded;
        
        void OnLevelWasLoaded(int level)
        {
            var handler = LevelWasLoaded;
            if(handler != null)
            {
                handler(level);
            }
        }
        
        #endregion

        #endregion
    }
}
