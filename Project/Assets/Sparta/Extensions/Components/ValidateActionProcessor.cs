using System;
using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Components
{
    public interface IValidatedActionHandler<T, R>
    {
        void Handle(T action, R result);
    }

    public interface IActionValidator<T>
    {
        bool Validate(T action);
    }

    public interface IActionValidator<T, R>
    {
        bool Validate(T action, out R result);
    }

    public delegate bool ActionValidatorFunc<T, R>(T action, out R result);

    public class ValidateActionProcessor<T> : IActionHandler<T>
    {
        interface ITypeValidator
        {
            void RegisterObject(object obj);
            void UnregisterObject(object obj);
            void RegisterDelegate(object dlg);
            void UnregisterDelegate(object dlg);
            bool ValidateAction(object action, out object result);
        }

        class TypeValidator<K> : ITypeValidator
        {
            List<IActionValidator<K>> _validators = new List<IActionValidator<K>>();
            List<IActionValidator<K>> _validators = new List<IActionValidator<K>>();
            ActionValidatorFunc<K, object> _delegates;

            public void Register(object obj)
            {
                var handlerk = obj as IActionValidator<K>;
                if(handlerk != null)
                {
                    _delegates.Add(handlerk);
                }
            }

            public void UnregisterHandler(object handler)
            {
                var handlerk = handler as IActionHandler<K>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
            }

            public void RegisterActionHandler(object action)
            {
                var actionk = action as Action<K>;
                if(actionk != null)
                {
                    _actions += actionk;
                }
            }

            public void UnregisterActionHandler(object action)
            {
                var actionk = action as Action<K>;
                if(actionk != null)
                {
                    _actions -= actionk;
                }
            }

            public bool HandleAction(object action)
            {
                if(!(action is K))
                {
                    return false;
                }
                if(_handlers.Count == 0 && _actions == null)
                {
                    return false;
                }
                var kaction = (K)action;
                for(var i = 0; i < _handlers.Count; i++)
                {
                    _handlers[i].Handle(kaction);
                }
                if(_actions != null)
                {
                    _actions(kaction);
                }
                return true;
            }
        }

        /*
        interface ITypeHandler
        {
            void RegisterHandler(object handler);
            void UnregisterHandler(object handler);
            void RegisterDelegate(object dlg);
            void UnregisterDelegate(object dlg);
            bool HandleAction(object obj, R result);
        }

        class TypeHandler<K> : ITypeHandler
        {
            List<IStateActionHandler<S, K>> _handlers = new List<IStateActionHandler<S, K>>();
            Action<S, K> _delegates;

            public void RegisterHandler(object handler)
            {
                var handlerk = handler as IStateActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Add(handlerk);
                }
            }

            public void UnregisterHandler(object handler)
            {
                var handlerk = handler as IStateActionHandler<S, K>;
                if(handlerk != null)
                {
                    _handlers.Remove(handlerk);
                }
            }

            public void RegisterDelegate(object dlg)
            {
                var dlgk = dlg as Action<S, K>;
                if(dlgk != null)
                {
                    _delegates += dlgk;
                }
            }

            public void UnregisterAction(object dlg)
            {
                var dlgk = dlg as Action<S, K>;
                if(dlgk != null)
                {
                    _delegates -= dlgk;
                }
            }

            public bool HandleAction(S state, object action)
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

        public bool Handle(T action)
        {

        }

        public void Register<K>(IActionHandler<K> handler)
        {

        }

        public void Unregister<K>(IActionHandler<K> handler)
        {
           
        }

        public void Unregister<K>()
        {
        }

        public void Register<K>(Action<K> action)
        {
            
        }

        public void Unregister<K>(Action<K> action)
        {
        }

*/
    }

}
