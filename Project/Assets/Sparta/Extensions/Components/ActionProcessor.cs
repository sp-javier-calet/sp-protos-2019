using System;
using System.Collections.Generic;

namespace SocialPoint.Components
{
    public interface IActionHandler<T>
    {
        void Handle(T action);
    }

    public interface IActionHandler : IActionHandler<object>
    {
    }

    public class ActionProcessor<T> : IActionHandler<T>
    {
        interface ITypeProcessor
        {
            void Register(object obj);
            void Unregister(object dlg);
            bool Process(object action);
        }

        class TypeProcessor<K> : ITypeProcessor
        {
            List<IActionHandler<K>> _handlers = new List<IActionHandler<K>>();
            Action<K> _delegates;

            public void Register(object obj)
            {
                var handlerk = obj as IActionHandler<K>;
                if(handlerk != null)
                {
                    _handlers.Add(handlerk);
                }
                var actionk = obj as Action<K>;
                if(actionk != null)
                {
                    _delegates += actionk;
                }
            }

            public void Unregister(object obj)
            {
                var handlerk = obj as IActionHandler<K>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
                var dlgk = obj as Action<K>;
                if(dlgk != null)
                {
                    _delegates -= dlgk;
                }
            }

            public bool Process(object action)
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
                    _handlers[i].Handle(kaction);
                }
                if(_delegates != null)
                {
                    _delegates(kaction);
                }
                return true;
            }
        }

        Dictionary<Type, ITypeProcessor> _types;

        public ActionProcessor()
        {
            _types = new Dictionary<Type, ITypeProcessor>();
        }

        void IActionHandler<T>.Handle(T action)
        {
            Process(action);
        }

        public bool Process(T action)
        {
            var handled = false;
            var itr = _types.GetEnumerator();
            while (itr.MoveNext())
            {
                if(itr.Current.Value.Process(action))
                {
                    handled = true;
                }
            }
            itr.Dispose();
            return handled;
        }

        public void Register<K>(IActionHandler<K> handler)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(!_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor = new TypeProcessor<K>();
                _types[type] = typeProcessor;
            }
            typeProcessor.Register(handler);
        }

        public void Unregister<K>(IActionHandler<K> handler)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor.Register(handler);
            }
        }

        public void Unregister<K>()
        {
            _types.Remove(typeof(K));
        }

        public void Register<K>(Action<K> dlg)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(!_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor = new TypeProcessor<K>();
                _types[type] = typeProcessor;
            }
            typeProcessor.Register(dlg);
        }

        public void Unregister<K>(Action<K> dlg)
        {
            var type = typeof(K);
            ITypeProcessor typeProcessor;
            if(_types.TryGetValue(type, out typeProcessor))
            {
                typeProcessor.Register(dlg);
            }
        }
    }
}
