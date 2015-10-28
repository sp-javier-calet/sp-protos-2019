using System;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.ServerSync
{
    public delegate Attr SyncDelegate();

    public interface ICommandQueue : IDisposable
    {
        SyncDelegate AutoSync { set; }

        bool AutoSyncEnabled { set; }

        bool Synced { get; }

        void Start();

        void Stop();

        void Reset();

        void Add(Command cmd, Action callback);

        void Add(Command cmd, ErrorDelegate callback = null);

        int Remove(Packet.FilterDelegate callback = null);

        void Flush(Action callback);

        void Flush(Packet.FinishDelegate callback = null);

        void Send(Action finish = null);
    }
}