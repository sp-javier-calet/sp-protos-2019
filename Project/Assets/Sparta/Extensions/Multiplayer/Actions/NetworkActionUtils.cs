using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class NetworkActionUtils
    {
        public static void ApplyAction(NetworkActionTuple actionTuple, Dictionary<Type, List<INetworkActionDelegate>> actionDelegates, NetworkScene scene)
        {
            List<INetworkActionDelegate> actionCallbackList;
            if(actionDelegates.TryGetValue(actionTuple.ActionType, out actionCallbackList))
            {
                var itr = actionCallbackList.GetEnumerator();
                while(itr.MoveNext())
                {
                    INetworkActionDelegate actionCallback = itr.Current;
                    actionCallback.ApplyAction(actionTuple.Action, scene);
                }
                itr.Dispose();
            }
        }
    }
}
