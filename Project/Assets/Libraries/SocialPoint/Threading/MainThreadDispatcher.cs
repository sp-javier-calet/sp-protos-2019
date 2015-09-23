using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Threading
{
    public class MainThreadDispatcher : MonoBehaviour
    {

        private static MainThreadDispatcher _instance = null;
        private static IList<Action> _pendingActions = new List<Action>();
        
        public static void Init()
        { 
            if(_instance == null)
            {
                GameObject go = new GameObject();
                _instance = go.AddComponent<MainThreadDispatcher>();
                go.name = _instance.GetType().ToString();
                DontDestroyOnLoad(_instance.gameObject);
            }
        }

        public static void Dispatch(Action action)
        {
            if(_instance == null)
            {
                throw new InvalidOperationException("MainThreadDispatcher not initialized.");
            }
            lock(_pendingActions)
            {
                _pendingActions.Add(action);
            }
        }

        private void RunPendingActions()
        {
            lock(_pendingActions)
            {
                foreach(Action action in _pendingActions)
                {
                    action();
                }
                _pendingActions.Clear();
            }
        }

        void LateUpdate()
        {
            RunPendingActions();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            RunPendingActions();
        }
    }
}
