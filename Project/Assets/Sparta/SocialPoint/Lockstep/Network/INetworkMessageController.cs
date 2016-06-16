using System;
using System.IO;
using SocialPoint.Lockstep.Network;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public class NetworkMessageData
    {
        public IReaderWrapper Reader;
        public int ConnectionId;
    }

    public class SyncNetworkMessageData : NetworkMessageData
    {
        public int ServerDelay;
    }

    public enum NetworkChannel
    {
        Reliable,
        Unreliable
    }

    public interface INetworkMessageController
    {
        void RegisterHandler(short msgType, Action<NetworkMessageData> handler);

        void RegisterSyncHandler(short msgType, Action<SyncNetworkMessageData> handler);

        void UnregisterHandler(short msgType);

        void Send(short msgType, INetworkMessage msg, NetworkChannel channel = NetworkChannel.Reliable, int connectionId = 0);

        void SendToAll(short msgType, INetworkMessage msg, NetworkChannel channel = NetworkChannel.Reliable);
    }
}