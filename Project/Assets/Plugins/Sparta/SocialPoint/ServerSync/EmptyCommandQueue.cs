using System;
using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class EmptyCommandQueue : ICommandQueue
    {
        public int SendInterval { get; set; }

        public bool PingEnabled { get; set; }

        public SyncDelegate AutoSync{ set; private get; }

        public event CommandResponseDelegate CommandResponse
        {
            add { }
            remove { }
        }

        public event CommandQueueErrorDelegate GeneralError
        {
            add { }
            remove { }
        }

        public event CommandErrorDelegate CommandError
        {
            add { }
            remove { }
        }

        
        public event Action SyncChange
        {
            add { }
            remove { }
        }

        public bool AutoSyncEnabled { set { } }

        public bool Synced { get { return true; } }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }

        public void Add(Command cmd, Action<Attr, Error> callback)
        {
        }

        public int Remove(Packet.FilterDelegate callback = null)
        {
            return 0;
        }

        public void Flush(Action callback)
        {
        }

        public void Flush(Packet.FinishDelegate callback = null)
        {
        }

        public void Send(Action finish = null)
        {
        }
    }
}
