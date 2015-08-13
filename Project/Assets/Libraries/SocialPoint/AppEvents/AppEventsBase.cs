using System;
using UnityEngine;

namespace SocialPoint.AppEvents
{
    /// <summary>
    /// Manage interface events and provides a common base implementation for all platform-dependent classes
    /// </summary>
    public abstract class AppEventsBase : MonoBehaviour, IAppEvents 
    {
        #region IAppEvents implementation

        #region Native Events
        public event Action WillGoBackground;
        
        /// <summary>
        /// Occurs when setup going to background.
        /// </summary>
        protected  void OnWillGoBackground()
        {
            var handler = WillGoBackground;
            if(handler != null)
            {
                handler();
            }
        }

        public event Action GoBackground;

        /// <summary>
        /// Occurs when going to background.
        /// </summary>
        protected  void OnGoBackground()
        {
            var handler = GoBackground;
            if(handler != null)
            {
                handler();
            }
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