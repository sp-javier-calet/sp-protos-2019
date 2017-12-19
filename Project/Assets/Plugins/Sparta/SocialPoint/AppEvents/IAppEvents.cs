using System;
using SocialPoint.Utils;
using UnityEngine.SceneManagement;

namespace SocialPoint.AppEvents
{
    public interface IAppEvents : IDisposable
    {
        /// <summary>
        /// Occurs before going to background.
        /// </summary>
        PriorityAction WillGoBackground{ get; }

        /// <summary>
        /// Occurs when was on background.
        /// </summary>
        PriorityAction WasOnBackground{ get; }

        /// <summary>
        /// Occurs after the game was loaded.
        /// </summary>
        PriorityAction GameWasLoaded{ get; }

        /// <summary>
        /// Occurs after the game was loaded finished all coroutines.
        /// </summary>
        PriorityCoroutineAction AfterGameWasLoaded{ get; }

        /// <summary>
        /// Occurs before game is restarted
        /// </summary>
        PriorityAction GameWillRestart{ get; }

        /// <summary>
        /// Occurs when the app was covered.
        /// for example when a popup appears on top of the app
        /// </summary>
        event Action WasCovered;

        /// <summary>
        /// Occurs when received memory warning.
        /// </summary>
        event Action ReceivedMemoryWarning;

        /// <summary>
        /// Occurs when app is opened with source data
        /// </summary>
        event Action<AppSource> OpenedFromSource;

        /// <summary>
        /// Occurs when application quits.
        /// </summary>
        event Action ApplicationQuit;

        /// <summary>
        /// The source info
        /// </summary>
        AppSource Source { get; }

        /// <summary>
        /// Trigger ReceivedMemoryWarning by hand (for debug purposes)
        /// </summary>
        void TriggerMemoryWarning();

        /// <summary>
        /// Trigger WillGoBackground by hand (for debug purposes)
        /// </summary>
        void TriggerWillGoBackground();

        /// <summary>
        /// Trigger WasOnBackground by hand (for debug purposes)
        /// </summary>
        void TriggerWasOnBackground();

        /// <summary>
        /// Trigger GameWasLoaded
        /// </summary>
        void TriggerGameWasLoaded();

        /// <summary>
        /// Trigger GameWillRestart
        /// </summary>
        void TriggerGameWillRestart();

        /// <summary>
        /// Trigger ApplicationQuit
        /// </summary>
        void TriggerApplicationQuit();
    }

    public static class AppEventsExtension
    {
        public static void RestartGame(this IAppEvents events)
        {
            events.RestartGame(0);
        }

        public static void RestartGame(this IAppEvents events, string restartScene)
        {
            events.TriggerGameWillRestart();
            SceneManager.LoadScene(restartScene);
        }

        public static void RestartGame(this IAppEvents events, int restartScene)
        {
            events.TriggerGameWillRestart();
            SceneManager.LoadScene(restartScene);
        }

        public static bool QuitGame(this IAppEvents events)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            return SocialPoint.Base.AndroidContext.CurrentActivity.Call<bool>("moveTaskToBack", true);
            #else
            return false;
            #endif
        }

        public static void KillGame(this IAppEvents events)
        {
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            events.TriggerApplicationQuit();
            try
            {
                // Remove activity from task manager. Requires Level API 21.
                SocialPoint.Base.AndroidContext.CurrentActivity.Call("finishAndRemoveTask");
            }
            catch(Exception)
            {
                SocialPoint.Base.Log.w("finishAndRemoveTask not available");
            }

            System.Diagnostics.Process.GetCurrentProcess().Kill();
#else
            UnityEngine.Application.Quit();
#endif
        }

        [Obsolete("Use WillGoBackground property")]
        public static void RegisterWillGoBackground(this IAppEvents events, int priority, Action action)
        {
            events.WillGoBackground.Add(priority, action);
        }

        [Obsolete("Use WillGoBackground property")]
        public static void RegisterWillGoBackground(this IAppEvents events, Action<int> action)
        {
            events.WillGoBackground.Add(action);
        }

        [Obsolete("Use WillGoBackground property")]
        public static void UnregisterWillGoBackground(this IAppEvents events, Action action)
        {
            events.WillGoBackground.Remove(action);
        }

        [Obsolete("Use WillGoBackground property")]
        public static void UnregisterWillGoBackground(this IAppEvents events, Action<int> action)
        {
            events.WillGoBackground.Remove(action);
        }

        [Obsolete("Use GameWasLoaded property")]
        public static void RegisterGameWasLoaded(this IAppEvents events, int priority, Action action)
        {
            events.GameWasLoaded.Add(priority, action);
        }

        [Obsolete("Use GameWasLoaded property")]
        public static void RegisterGameWasLoaded(this IAppEvents events, Action<int> action)
        {
            events.GameWasLoaded.Add(action);
        }

        [Obsolete("Use GameWasLoaded property")]
        public static void UnregisterGameWasLoaded(this IAppEvents events, Action action)
        {
            events.GameWasLoaded.Remove(action);
        }

        [Obsolete("Use GameWasLoaded property")]
        public static void UnregisterGameWasLoaded(this IAppEvents events, Action<int> action)
        {
            events.GameWasLoaded.Remove(action);
        }

        [Obsolete("Use GameWillRestart property")]
        public static void RegisterGameWillRestart(this IAppEvents events, int priority, Action action)
        {
            events.GameWillRestart.Add(priority, action);
        }

        [Obsolete("Use GameWillRestart property")]
        public static void RegisterGameWillRestart(this IAppEvents events, Action<int> action)
        {
            events.GameWillRestart.Add(action);
        }

        [Obsolete("Use GameWillRestart property")]
        public static void UnregisterGameWillRestart(this IAppEvents events, Action action)
        {
            events.GameWillRestart.Remove(action);
        }

        [Obsolete("Use GameWillRestart property")]
        public static void UnregisterGameWillRestart(this IAppEvents events, Action<int> action)
        {
            events.GameWillRestart.Remove(action);
        }
    }
}
