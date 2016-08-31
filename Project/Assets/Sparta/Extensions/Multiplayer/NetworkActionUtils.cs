using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    class NetworkActionUtils
    {
        /// <summary>
        /// Helper function to search for available delegates asociated to an action and apply them to an scene.
        /// </summary>
        /// <returns><c>true</c>, if action was applyed, <c>false</c> otherwise.</returns>
        /// <param name="actionType">Action type.</param>
        /// <param name="action">Action.</param>
        /// <param name="actionDelegates">Action delegates.</param>
        /// <param name="scene">Scene.</param>
        public static bool ApplyAction(Type actionType, object action, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates, NetworkScene scene)
        {
            bool applied = false;

            if(action is IAppliableSceneAction)
            {
                ((IAppliableSceneAction)action).Apply(scene);
                applied = true;
            }

            List<INetworkActionDelegate> actionCallbackList;
            if(actionDelegates.TryGetValue(actionType, out actionCallbackList))
            {
                var itr = actionCallbackList.GetEnumerator();
                while(itr.MoveNext())
                {
                    INetworkActionDelegate actionCallback = itr.Current;
                    actionCallback.ApplyAction(action, scene);
                    applied = true;
                }
                itr.Dispose();
            }

            return applied;
        }

        /// <summary>
        /// Helper function to register an action delegate
        /// </summary>
        /// <param name="actionType">Action type.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="actionDelegates">Action delegates.</param>
        public static void RegisterActionDelegate(Type actionType, INetworkActionDelegate callback, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates)
        {
            List<INetworkActionDelegate> actionCallbackList;
            if(actionDelegates.TryGetValue(actionType, out actionCallbackList))
            {
                actionCallbackList.Add(callback);
            }
            else
            {
                actionCallbackList = new List<INetworkActionDelegate>();
                actionCallbackList.Add(callback);
                actionDelegates.Add(actionType, actionCallbackList);
            }
        }

        public static void RegisterActionDelegate<T>(Action<T, NetworkScene> callback, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates)
        {
            RegisterActionDelegate(typeof(T), new NetworkActionDelegate<T>(callback), actionDelegates);
        }

        public static bool UnregisterActionDelegate(Type actionType, INetworkActionDelegate callback, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates)
        {
            List<INetworkActionDelegate> actionCallbackList;
            if(actionDelegates.TryGetValue(actionType, out actionCallbackList))
            {
                return actionCallbackList.RemoveAll(dlg => dlg.Equals(callback)) > 0;
            }
            return false;
        }

        public static bool UnregisterActionDelegate<T>(Action<T, NetworkScene> callback, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates)
        {
            return UnregisterActionDelegate(typeof(T), new NetworkActionDelegate<T>(callback), actionDelegates);
        }
    }
}
