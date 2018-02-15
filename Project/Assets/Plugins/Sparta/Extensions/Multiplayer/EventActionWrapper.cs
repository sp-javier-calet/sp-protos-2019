using System;

namespace SocialPoint.Multiplayer
{
    public struct EventActionWrapper
    {
        event Action _event;

        public void Add(Action action)
        {
            _event += action;
        }

        public void Remove(Action action)
        {
            _event -= action;
        }

        public void Clear()
        {
            _event = null;
        }

        public void Call()
        {
            if(_event != null)
            {
                _event();
            }
        }
    }
}