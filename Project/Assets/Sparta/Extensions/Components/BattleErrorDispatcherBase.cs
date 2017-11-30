using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Locale;

namespace SocialPoint.Components
{


    public class BattleError
    {
        public Error Error;
        public string Title;
        public string Description;
    }

    public class BattleErrorDispatcherBase : IBattleErrorDispatcher, IDisposable
    {
        

        public BattleErrorDispatcherBase()
        {
            _errorHandlers = new List<IBattleErrorHandler>();
        }

        public void RegisterHandler(IBattleErrorHandler handler)
        {
            _errorHandlers.Add(handler);
        }

        public void Dispose()
        {
            _errorHandlers.Clear();
        }

        public void DispatchError(BattleError battleError)
        {
            for(int i = 0; i < _errorHandlers.Count; i++)
            {
                _errorHandlers[i].OnError(battleError);
            }
        }
    }
}
