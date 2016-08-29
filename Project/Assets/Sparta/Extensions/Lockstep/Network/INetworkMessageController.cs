﻿using System;
using SocialPoint.Lockstep.Network;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public class NetworkMessageData
    {
        public IReader Reader;
        public int ConnectionId;
    }

    public sealed class SyncNetworkMessageData : NetworkMessageData
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