using System;

namespace SocialPoint.ServerSync
{
    public interface ICommandQueue : IDisposable
    {
		bool Synced { get; }

        void Start();
        void Stop();
        void Reset();
        void Add(Command cmd, Action callback);
        void Add(Command cmd, PackedCommand.FinishDelegate callback=null);
        int Remove(Packet.FilterDelegate callback = null);
        void Flush(Action callback);
        void Flush(Packet.FinishDelegate callback = null);
        void Send(Action finish = null);
    }
}