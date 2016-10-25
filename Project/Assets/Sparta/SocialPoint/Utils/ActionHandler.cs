using System;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    public interface IActionHandler<S, T>
    {
        void HandleAction(S state, T action);
    }

    public interface IActionHandler<S> : IActionHandler<S, object>
    {
    }

    public class ActionHandler<S, T>
    {
        interface ITypeHandler
        {
            void RegisterHandler(object handler);
            void UnregisterHandler(object handler);
            void RegisterAction(object action);
            void UnregisterAction(object action);
            bool HandleAction(S state, object obj);
        }

        class TypeHandler<K> : ITypeHandler
        {
            List<IActionHandler<S, K>> _handlers = new List<IActionHandler<S, K>>();
            Action<S, K> _actions;

            public void RegisterHandler(object handler)
            {
                var handlerk = handler as IActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Add(handlerk);
                }
            }

            public void UnregisterHandler(object handler)
            {
                var handlerk = handler as IActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
            }

            public void RegisterAction(object action)
            {
                var actionk = action as Action<S, K>;
                if(actionk != null)
                {
                    _actions += actionk;
                }
            }

            public void UnregisterAction(object action)
            {
                var actionk = action as Action<S, K>;
                if(actionk != null)
                {
                    _actions -= actionk;
                }
            }

            public bool HandleAction(S state, object obj)
            {
                if(!(obj is K))
                {
                    return false;
                }
                if(_handlers.Count == 0 && _actions == null)
                {
                    return false;
                }
                var action = (K)obj;
                for(var i = 0; i < _handlers.Count; i++)
                {
                    _handlers[i].HandleAction(state, action);
                }
                if(_actions != null)
                {
                    _actions(state, action);
                }
                return true;
            }
        }

        Dictionary<Type, ITypeHandler> _handlers;

        public ActionHandler()
        {
            _handlers = new Dictionary<Type, ITypeHandler>();
        }

        public bool HandleAction(S state, T evnt)
        {
            var handled = false;
            var itr = _handlers.GetEnumerator();
            while (itr.MoveNext())
            {
                if(itr.Current.Value.HandleAction(state, evnt))
                {
                    handled = true;
                }
            }
            itr.Dispose();
            return handled;
        }

        public void Register<K>(IActionHandler<S, K> handler) where K: T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(!_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                _handlers[type] = typeHandler;
            }
            typeHandler.RegisterHandler(handler);
        }

        public void Unregister<K>(IActionHandler<S, K> handler) where K: T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.UnregisterHandler(handler);
            }
        }

        public void Register<K>(Action<S, K> action) where K: T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(!_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                _handlers[type] = typeHandler;
            }
            typeHandler.RegisterAction(action);
        }

        public void Unregister<K>(Action<S, K> action) where K: T
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.UnregisterAction(action);
            }
        }
    }

    public class ActionHandler<S> : ActionHandler<S, object>
    {
    }

}

