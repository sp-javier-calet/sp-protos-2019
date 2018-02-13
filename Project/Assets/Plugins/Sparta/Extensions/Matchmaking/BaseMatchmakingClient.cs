﻿using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.Matchmaking
{
    public abstract class BaseMatchmakingClient : IMatchmakingClient
    {
        protected readonly List<IMatchmakingClientDelegate> _delegates = new List<IMatchmakingClientDelegate>();
        protected readonly List<IMatchmakingClientDelegate> _delegatesToAdd = new List<IMatchmakingClientDelegate>();
        protected readonly List<IMatchmakingClientDelegate> _delegatesToRemove = new List<IMatchmakingClientDelegate>();

        public virtual void Start()
        {
            TriggerOnStart();
        }

        public void Start(AttrDic extraData, bool searchForActiveMatch, string connectId)
        {
            Start();
        }

        public void Clear()
        {
            
        }

        public string Room { get; set; }

        public virtual void Stop()
        {
            TriggerOnStopped(true);
        }

        void SyncDelegates()
        {
            for(int i = 0; i < _delegatesToRemove.Count; ++i)
            {
                _delegates.Remove(_delegatesToRemove[i]);
            }
            _delegatesToRemove.Clear();
            _delegates.AddRange(_delegatesToAdd);
            _delegatesToAdd.Clear();
        }

        protected void TriggerOnStart()
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStart();
            }
        }

        protected void TriggerOnError(Error err)
        {
            if(err == null)
            {
                return;
            }
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        protected void TriggerOnMatched(Match match)
        {
            if(match == null)
            {
                return;
            }
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatched(match);
            }
        }

        protected void TriggerOnWaiting(int time)
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnWaiting(time);
            }
        }

        protected void TriggerOnStopped(bool stopped)
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStopped(stopped);
            }
        }

        public void AddDelegate(IMatchmakingClientDelegate dlg)
        {
            if(_delegatesToRemove.Contains(dlg))
            {
                _delegatesToRemove.Remove(dlg);
            }
            else
            {
                _delegatesToAdd.Add(dlg);
            }
        }

        public void RemoveDelegate(IMatchmakingClientDelegate dlg)
        {
            if(_delegatesToAdd.Contains(dlg))
            {
                _delegatesToAdd.Remove(dlg);
            }
            else
            {
                _delegatesToRemove.Add(dlg);
            }
        }
    }
}