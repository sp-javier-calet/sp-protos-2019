using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkActionTuple
    {
        public Type ActionType;
        public object Action;

        public NetworkActionTuple(Type actionType, object action)
        {
            ActionType = actionType;
            Action = action;
        }
    }
}
