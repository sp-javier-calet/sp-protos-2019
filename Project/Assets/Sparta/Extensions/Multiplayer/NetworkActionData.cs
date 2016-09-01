using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkActionData
    {
        public Type ActionType;
        public object Action;

        public NetworkActionData(Type actionType, object action)
        {
            ActionType = actionType;
            Action = action;
        }
    }
}
