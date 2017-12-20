using SocialPoint.Base;
using SocialPoint.Attributes;
using System;
using System.Collections.Generic;

namespace SocialPoint.Matchmaking
{
    public interface IMatchStorage
    {
        bool Stored{ get; }

        bool Load(out Match match);

        void Save(Match match);

        void Clear();
    }

    public class AttrMatchStorage : IMatchStorage
    {
        const string DefaultStorageKey = "matchmaking";

        string _storageKey;
        IAttrStorage _storage;

        public AttrMatchStorage(IAttrStorage storage, string storageKey = null)
        {
            if(string.IsNullOrEmpty(storageKey))
            {
                storageKey = DefaultStorageKey;
            }
            _storageKey = storageKey;
            _storage = storage;
        }

        public bool Stored
        {
            get
            {
                return _storage.Has(_storageKey);
            }
        }

        public bool Load(out Match match)
        {
            match = new Match();
            if(!_storage.Has(_storageKey))
            {
                return false;
            }
            var attr = _storage.Load(_storageKey);
            if(attr == null)
            {
                return false;
            }

            var attrDic = attr.AsDic;
            match.ParseAttrDic(attrDic);
                
            return true;
        }

        public void Save(Match match)
        {
            _storage.Save(_storageKey, match.ToAttrDic());
        }

        public void Clear()
        {
            _storage.Remove(_storageKey);
        }
    }

    public class StoredMatchmakingClient : IMatchmakingClient, IMatchmakingClientDelegate
    {
        List<IMatchmakingClientDelegate> _delegates;
        IMatchmakingClient _client;
        IMatchStorage _storage;

        public string Room
        {
            get
            {
                return _client.Room;
            }

            set
            {
                _client.Room = value;
            }
        }

        public StoredMatchmakingClient(IMatchmakingClient client, IMatchStorage storage = null)
        {
            _client = client;
            _storage = storage;
            _client.AddDelegate(this);
            _delegates = new List<IMatchmakingClientDelegate>();
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
                return;
            }
            _client.Start(extraData, searchForActiveMatch, connectId);
        }

        public void Stop()
        {
            _client.Stop();
        }

        public void Clear()
        {
            _client.Clear();
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
