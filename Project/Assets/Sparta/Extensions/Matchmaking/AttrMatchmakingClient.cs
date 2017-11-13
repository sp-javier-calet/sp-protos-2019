using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Matchmaking
{
    public class LogicGenericDataMatchStorage : IMatchStorage
    {
        AttrDic _matchData;

        public LogicGenericDataMatchStorage(AttrDic matchData)
        {
            _matchData = matchData;
        }

        public bool Stored
        {
            get
            {
                return _matchData != null;
            }
        }

        public bool Load(out Match match)
        {
            match = new Match();
            if(_matchData == null)
            {
                return false;
            }

            match.ParseAttrDic(_matchData);
            return true;
        }

        public void Save(Match match)
        {
        }

        public void Clear()
        {
            _matchData = null;
        }
    }

    public class AttrMatchmakingClient : IMatchmakingClient, IMatchmakingClientDelegate
    {
        List<IMatchmakingClientDelegate> _delegates;
        IMatchStorage _storage;

        public string Room{ get; set; }

        public AttrMatchmakingClient(IMatchStorage storage = null)
        {
            _delegates = new List<IMatchmakingClientDelegate>();
            _storage = storage;
        }

        public bool Stored
        {
            get
            {
                return _storage.Stored;
            }
        }

        public void AddDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void Start(AttrDic extraData, bool searchForActiveMatch, string connectId)
        {
            Match match;
            if(_storage.Load(out match))
            {
                OnMatched(match);
            }
        }

        public void Stop()
        {
        }

        public void Clear()
        {
            _storage.Clear();
        }

        void IMatchmakingClientDelegate.OnStart()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStart();
            }
        }

        void IMatchmakingClientDelegate.OnSearchOpponent()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnSearchOpponent();
            }
        }

        void IMatchmakingClientDelegate.OnWaiting(int waitTime)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnWaiting(waitTime);
            }
        }

        void IMatchmakingClientDelegate.OnMatched(Match match)
        {
            _storage.Save(match);
            OnMatched(match);
        }

        void IMatchmakingClientDelegate.OnStopped(bool successful)
        {
        }

        void IMatchmakingClientDelegate.OnError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        void OnMatched(Match match)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatched(match);
            }
        }

    }
}
