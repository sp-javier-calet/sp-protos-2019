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
        interface ITypeHandler
        {
            void Register(object obj);
            void Unregister(object dlg);
            bool Handle(object action);
        }

        class TypeHandler<K> : ITypeHandler
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

            public bool Handle(object action)
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

        Dictionary<Type, ITypeHandler> _handlers;

        public ActionProcessor()
        {
            _handlers = new Dictionary<Type, ITypeHandler>();
        }

        void IActionHandler<T>.Handle(T action)
        {
            Process(action);
        }

        public bool Process(T action)
        {
            var handled = false;
            var itr = _handlers.GetEnumerator();
            while (itr.MoveNext())
            {
                if(itr.Current.Value.Handle(action))
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
            if(!_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler = new TypeHandler<K>();
                _handlers[type] = typeHandler;
            }
            typeHandler.Register(obj);
        }

        void DoUnregister<K>(object obj)
        {
            var type = typeof(K);
            ITypeHandler typeHandler;
            if(_handlers.TryGetValue(type, out typeHandler))
            {
                typeHandler.Unregister(obj);
            }
        }

        public void Unregister<K>()
        {
            _handlers.Remove(typeof(K));
        }

        public void Register<K>(IActionHandler<K> handler)
        {
            DoRegister<K>(handler);
        }

        public void Unregister<K>(IActionHandler<K> handler)
        {
            DoUnregister<K>(handler);
        }

        public void Register<K>(Action<K> dlg)
        {
            DoRegister<K>(dlg);
        }

        public void Unregister<K>(Action<K> dlg)
        {
            DoUnregister<K>(dlg);
        }
    }

    public class ActionProcessor : ActionProcessor<object>
    {
    }
}
