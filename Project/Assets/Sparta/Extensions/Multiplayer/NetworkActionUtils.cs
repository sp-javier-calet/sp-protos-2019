using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class NetworkActionUtils
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
    }
}
