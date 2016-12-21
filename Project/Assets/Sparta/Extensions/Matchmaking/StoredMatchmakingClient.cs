using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Utils;
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
        const string MatchIdAttrKey = "match_id";
        const string PlayerIdAttrKey = "player_id";
        const string GameInfoAttrKey = "game_info";
        const string ServerInfoAttrKey = "server_info";

        string _storageKey;
        IAttrStorage _storage;

        public AttrMatchStorage(IAttrStorage storage, string storageKey=null)
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
            if(!_storage.Has(_storageKey))
            {
                match = new Match();
                return false;
            }
            var attr = _storage.Load(_storageKey);
            if(attr == null)
            {
                match = new Match();
                return false;
            }
            var attrDic = attr.AsDic;
            match = new Match {
                Id = attrDic.GetValue(MatchIdAttrKey).ToString(),
                PlayerId = attrDic.GetValue(PlayerIdAttrKey).ToString(),
                Running = true,
                GameInfo = attrDic.Get(GameInfoAttrKey),
                ServerInfo = attrDic.Get(ServerInfoAttrKey),
            };
            return true;
        }

        public void Save(Match match)
        {
            var attr = new AttrDic();
            attr.SetValue(MatchIdAttrKey, match.Id);
            attr.SetValue(PlayerIdAttrKey, match.PlayerId);
            attr.Set(GameInfoAttrKey, match.GameInfo);
            attr.Set(ServerInfoAttrKey, match.ServerInfo);
            _storage.Save(_storageKey, attr);
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

        public StoredMatchmakingClient(IMatchmakingClient client, IMatchStorage storage=null)
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

        public void Start()
        {
            Match match;
            if(_storage.Load(out match))
            {
                OnMatched(match);
                return;
            }
            _client.Start();
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

        void IMatchmakingClientDelegate.OnWaiting(int waitTime)
        {
            for(var i=0; i<_delegates.Count; i++)
            {
                _delegates[i].OnWaiting(waitTime);
            }
        }

        void IMatchmakingClientDelegate.OnMatched(Match match)
        {
            _storage.Save(match);
            OnMatched(match);
        }

        void IMatchmakingClientDelegate.OnError(Error err)
        {
            for(var i=0; i<_delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        void OnMatched(Match match)
        {
            for(var i=0; i<_delegates.Count; i++)
            {
                _delegates[i].OnMatched(match);
            }
        }

    }

}
