using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.ServerSync
{
    public delegate Attr SyncDelegate();

    public enum CommandQueueErrorType
    {
        HttpResponse,
        InvalidJson,
        ResponseJson,
        SessionLost,
        OutOfSync,
        Exception,
        ClockChange
    }

    public delegate void CommandQueueErrorDelegate(CommandQueueErrorType type, Error err);
    public delegate void CommandErrorDelegate(Command cmd, Error err, Attr resp);
    public delegate void CommandResponseDelegate(Command cmd, Attr resp);

    public interface ICommandQueue : IDisposable
    {
        int SendInterval { get; set; }
        bool PingEnabled { get; set; }

        SyncDelegate AutoSync{ set; }

        event CommandQueueErrorDelegate GeneralError;
        event CommandErrorDelegate CommandError;
        event CommandResponseDelegate CommandResponse;
        event Action SyncChange;

        bool AutoSyncEnabled { set; }

        bool Synced { get; }

        void Start();

        void Stop();

        void Reset();

        void Add(Command cmd, Action<Attr, Error> callback = null);

        int Remove(Packet.FilterDelegate callback = null);

        void Flush(Action callback);

        void Flush(Packet.FinishDelegate callback = null);

        void Send(Action finish = null);
    }
}
