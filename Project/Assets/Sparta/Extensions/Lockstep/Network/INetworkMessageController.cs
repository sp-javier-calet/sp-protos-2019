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

    public enum NetworkReliability
    {
        Reliable,
        Unreliable
    }

    public interface INetworkMessageController
    {
        void RegisterHandler(byte msgType, Action<NetworkMessageData> handler);

        void RegisterSyncHandler(byte msgType, Action<SyncNetworkMessageData> handler);

        void UnregisterHandler(byte msgType);

        void Send(byte msgType, INetworkMessage msg, NetworkReliability channel = NetworkReliability.Reliable, int connectionId = 0);

        void SendToAll(byte msgType, INetworkMessage msg, NetworkReliability channel = NetworkReliability.Reliable);
    }
}