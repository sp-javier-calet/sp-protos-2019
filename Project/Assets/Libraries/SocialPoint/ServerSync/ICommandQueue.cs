﻿using System;
using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public delegate Attr SyncDelegate();

    public enum CommandQueueErrorType
    {
        HttpResponse,
        InvalidJson,
        ResponseJson,
        SessionLost,
        OutOfSync
    }   

    public delegate void CommandQueueErrorDelegate(CommandQueueErrorType type, Error err);    
    public delegate void CommandErrorDelegate(Command cmd, Error err, Attr resp);

    public interface ICommandQueue : IDisposable
    {
        SyncDelegate AutoSync{ set; }
        event CommandQueueErrorDelegate GeneralError;
        event CommandErrorDelegate CommandError;

        bool AutoSyncEnabled { set; }

		bool Synced { get; }

        void Start();
        void Stop();
        void Reset();
        void Add(Command cmd, Action callback);
        void Add(Command cmd, ErrorDelegate callback=null);
        int Remove(Packet.FilterDelegate callback = null);
        void Flush(Action callback);
        void Flush(Packet.FinishDelegate callback = null);
        void Send(Action finish = null);
    }
}