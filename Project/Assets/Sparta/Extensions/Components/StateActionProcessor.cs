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
        public void HandleAction(S state, IStateAppliable<S> action)
        {
            if(action != null)
            {
                action.Apply(state);
            }
        }
    }

    public class StateActionProcessor<S, T> : IStateActionHandler<S, T>
    {
        interface ITypeProcessor
        {
            void Register(object obj);
            void Unregister(object obj);
            bool Process(S state, object action);
        }

        class TypeHandler<K> : ITypeProcessor
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

            public bool Process(S state, object action)
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

        Dictionary<Type, ITypeProcessor> _types;

        public StateActionProcessor()
        {
            _types = new Dictionary<Type, ITypeProcessor>();
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
                if(itr.Current.Value.Process(state, action))
                {
                    handled = true;
                }
            }
            itr.Dispose();
            return handled;
        }

        public void Register<K>(IStateActionHandler<S, K> handler)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(!_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor = new TypeHandler<K>();
                _types[type] = typeProcessor;
            }
            typeProcessor.Register(handler);
        }

        public void Unregister<K>(IStateActionHandler<S, K> handler)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor.Unregister(handler);
            }
        }

        public void Unregister<K>()
        {
            _types.Remove(typeof(K));
        }

        public void Register<K>(Action<S, K> dlg)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(!_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor = new TypeHandler<K>();
                _types[type] = typeProcessor;
            }
            typeProcessor.Register(dlg);
        }

        public void Unregister<K>(Action<S, K> dlg)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor.Unregister(dlg);
            }
        }
    }

    public class StateActionProcessor<S> : StateActionProcessor<S, object>
    {
    }
}
