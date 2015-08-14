using System;
using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public class EmptyCommandQueue : ICommandQueue
    {
        public SyncDelegate AutoSync { set{} }
        public bool AutoSyncEnabled { set{} }
        public bool Synced { get{ return true; } }

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

        public void Add(Command cmd, Action callback)
        {
        }

        public void Add(Command cmd, PackedCommand.FinishDelegate callback=null)
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