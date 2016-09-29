using System;
using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommand : INetworkShareable, ICloneable
    {
    }   

    public interface ILockstepCommandLogic<T>
    {
        void Apply(T data);
    }

    public class ActionLockstepCommandLogic<T> : ILockstepCommandLogic<T>
    {
        Action<T> _action;

        public ActionLockstepCommandLogic(Action<T> action)
        {
            _action = action;
        }

        public void Apply(T data)
        {
            if(_action != null)
            {
                _action(data);
            }
        }
    }

    public interface ILockstepCommandLogic : ILockstepCommandLogic<ILockstepCommand>
    {
    }

    public class LockstepCommandLogic<T> : ILockstepCommandLogic
    {
        ILockstepCommandLogic<T> _inner;

        public LockstepCommandLogic(Action<T> action) :
            this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(ILockstepCommandLogic<T> inner)
        {
            _inner = inner;
        }

        public void Apply(ILockstepCommand data)
        {
            if(data is T && _inner != null)
            {
                _inner.Apply((T)data);
            }
        }
    }
}