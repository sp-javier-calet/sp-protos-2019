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
        /// <param name="actionTuple">Action tuple.</param>
        /// <param name="actionDelegates">Action delegates.</param>
        /// <param name="scene">Scene.</param>
        public static bool ApplyAction(NetworkActionTuple actionTuple, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates, NetworkScene scene)
        {
            bool applyed = false;
            List<INetworkActionDelegate> actionCallbackList;
            if(actionDelegates.TryGetValue(actionTuple.ActionType, out actionCallbackList))
            {
                var itr = actionCallbackList.GetEnumerator();
                while(itr.MoveNext())
                {
                    INetworkActionDelegate actionCallback = itr.Current;
                    actionCallback.ApplyAction(actionTuple.Action, scene);
                    applyed = true;
                }
                itr.Dispose();
            }

            return applyed;
        }

        /// <summary>
        /// Helper function to register an action delegate
        /// </summary>
        /// <param name="actionType">Action type.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="actionDelegates">Action delegates.</param>
        public static void RegisterAction(Type actionType, INetworkActionDelegate callback, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates)
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
