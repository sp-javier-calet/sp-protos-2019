using System;
using SocialPoint.Base;

namespace SocialPoint.Dependency
{
    public sealed class Listener<F> : IListener
    {
        event Action<F> _onResolved;

        public Listener<F> WhenResolved(Action<F> onResolved)
        {
            return WhenResolved<F>(onResolved);
        }

        public Listener<F> WhenResolved<T>(Action<T> onResolved) where T : F
        {
            _onResolved += instance => {
                try
                {
                    var casted = (T)instance;
                    onResolved(casted);
                }
                catch(Exception e)
                {
                    Log.x(e);
                }
            };
            return this;
        }

        public void OnResolved(object instance)
        {
            if(_onResolved != null)
            {
                _onResolved((F)instance);
            }
        }
    }
}