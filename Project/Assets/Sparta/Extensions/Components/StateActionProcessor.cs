using System;
using System.Collections.Generic;

namespace SocialPoint.Components
{    
    public interface IStateActionHandler<S, T>
    {
        void Handle(S state, T action);
    }

    public interface IStateActionHandler<S> : IStateActionHandler<S, object>
    {
    }

    public interface IStateAppliable<S>
    {
        void Apply(S state);
    }

    public class AppliableStateActionHandler<S> : IStateActionHandler<S, IStateAppliable<S>>
    {
        public void Handle(S state, IStateAppliable<S> action)
        {
            if(action != null)
            {
                action.Apply(state);
            }
        }
    }

    public class StateActionProcessor<S, T> : IStateActionHandler<S, T>
    {
        interface ITypeHandler
        {
            void Register(object obj);
            void Unregister(object obj);
            bool Handle(S state, object action);
        }

        class TypeHandler<K> : ITypeHandler
        {
            List<IStateActionHandler<S, K>> _handlers = new List<IStateActionHandler<S, K>>();
            Action<S, K> _delegates;

            public void Register(object obj)
            {
                var handlerk = obj as IStateActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Add(handlerk);
                }
                var dlgk = obj as Action<S, K>;
                if(dlgk != null)
                {
                    _delegates += dlgk;
                }
            }

            public void Unregister(object obj)
            {
                var handlerk = obj as IStateActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
                var dlgk = obj as Action<S, K>;
                if(dlgk != null)
                {
                    _delegates -= dlgk;
                }
            }

            public bool Handle(S state, object action)
            {
                if(!(action is K))
                {
                    return false;
                }
                if(_handlers.Count == 0 && _delegates == null)
                {
                    return false;
                }
                var kaction = (K)action;
                for(var i = 0; i < _handlers.Count; i++)
                {
                    _handlers[i].Handle(state, kaction);
                }
                if(_delegates != null)
                {
                    _delegates(state, kaction);
                }
                return true;
            }
        }

        Dictionary<Type, ITypeHandler> _types;

        public StateActionProcessor()
        {
            _types = new Dictionary<Type, ITypeHandler>();
            Register<IStateAppliable<S>>(new AppliableStateActionHandler<S>());
        }

        void IStateActionHandler<S, T>.Handle(S state, T action)
        {
            Process(state, action);
        }

        public bool Process(S state, T action)
        {
            var handled = false;
            var itr = _types.GetEnumerator();
            while (itr.MoveNext())
            {
                if(itr.Current.Value.Handle(state, action))
                {
                    handled = true;
                }
            }
            itr.Dispose();
            return handled;
        }

        void DoRegister<K>(object obj)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(!_types.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                _types[type] = typeHandler;
            }
            typeHandler.Register(obj);
        }

        void DoUnregister<K>(object obj)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_types.TryGetValue(type, out typeHandler))
            {
                typeHandler.Unregister(obj);
            }
        }

        public void Unregister<K>()
        {
            _types.Remove(typeof(K));
        }

        public void Register<K>(IStateActionHandler<S, K> handler)
        {
            DoRegister<K>(handler);
        }

        public void Unregister<K>(IStateActionHandler<S, K> handler)
        {
            DoUnregister<K>(handler);
        }

        public void Register<K>(Action<S, K> dlg)
        {
            DoRegister<K>(dlg);
        }

        public void Unregister<K>(Action<S, K> dlg)
        {
            DoUnregister<K>(dlg);
        }
    }

    public class StateActionProcessor<S> : StateActionProcessor<S, object>
    {
    }

    public class StateActionProcessor : StateActionProcessor<object>
    {
    }
}
